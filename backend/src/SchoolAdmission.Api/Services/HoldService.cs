using System.Collections.Concurrent;
using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Contracts.Responses;
using SchoolAdmission.Api.Domain.Entities;
using SchoolAdmission.Api.Domain.Enums;
using SchoolAdmission.Api.Domain.Exceptions;
using SchoolAdmission.Api.Infrastructure.Data;

namespace SchoolAdmission.Api.Services;

public class HoldService : IHoldService
{
    private readonly AdmissionDbContext _db;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _classLocks = new();

    public HoldService(AdmissionDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    private int GetHoldDurationSeconds()
    {
        var envVal = Environment.GetEnvironmentVariable("HOLD_DURATION_SECONDS");
        if (!string.IsNullOrWhiteSpace(envVal) && int.TryParse(envVal, out var seconds) && seconds > 0)
        {
            return seconds;
        }

        var configVal = _configuration.GetValue<int?>("HoldDurationSeconds");
        if (configVal.HasValue && configVal.Value > 0)
        {
            return configVal.Value;
        }

        return 600; // default 10 minutes
    }

    public async Task<HoldResponse> CreateHoldAsync(string classId, string studentId, CancellationToken ct = default)
    {
        var classLock = _classLocks.GetOrAdd(classId, _ => new SemaphoreSlim(1, 1));
        await classLock.WaitAsync(ct);

        try
        {
            var isRelational = _db.Database.IsRelational();
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = isRelational
                    ? await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)
                    : null;

                if (isRelational)
                {
                    // Pessimistic database row lock in PostgreSQL
                    await _db.Database.ExecuteSqlInterpolatedAsync(
                        $"SELECT \"Id\" FROM classes WHERE \"Id\" = {classId} FOR UPDATE;", ct);
                }

                var targetClass = await _db.Classes
                    .FirstOrDefaultAsync(c => c.Id == classId, ct);

                if (targetClass == null)
                {
                    throw new NotFoundException($"Class with ID '{classId}' was not found.");
                }

                var nowUtc = DateTime.UtcNow;

                // Check student constraint: at most one active hold or confirmed admission in this class
                var hasActiveOrConfirmed = await _db.Holds.AnyAsync(h =>
                    h.ClassId == classId &&
                    h.StudentId == studentId &&
                    (h.Status == HoldStatus.Confirmed ||
                     (h.Status == HoldStatus.Held && h.ExpiresAt > nowUtc)), ct);

                if (hasActiveOrConfirmed)
                {
                    throw new ConflictException("Student already has an active hold or confirmed admission in this class.");
                }

                // Check seat capacity invariant: confirmed + active held < totalSeats
                var confirmedCount = await _db.Holds.CountAsync(h =>
                    h.ClassId == classId && h.Status == HoldStatus.Confirmed, ct);

                var heldCount = await _db.Holds.CountAsync(h =>
                    h.ClassId == classId &&
                    h.Status == HoldStatus.Held &&
                    h.ExpiresAt > nowUtc, ct);

                if (confirmedCount + heldCount >= targetClass.TotalSeats)
                {
                    throw new ConflictException("No seats available for this class.");
                }

                var duration = GetHoldDurationSeconds();
                var newHold = new Hold
                {
                    ClassId = classId,
                    StudentId = studentId,
                    Status = HoldStatus.Held,
                    CreatedAt = nowUtc,
                    ExpiresAt = nowUtc.AddSeconds(duration)
                };

                _db.Holds.Add(newHold);
                await _db.SaveChangesAsync(ct);

                if (tx != null)
                {
                    await tx.CommitAsync(ct);
                }

                return new HoldResponse
                {
                    HoldId = newHold.Id,
                    ClassId = newHold.ClassId,
                    StudentId = newHold.StudentId,
                    Status = newHold.Status,
                    ExpiresAt = newHold.ExpiresAt.ToString("o")
                };
            });
        }
        finally
        {
            classLock.Release();
        }
    }

    public async Task<HoldDetailResponse> GetHoldByIdAsync(string holdId, CancellationToken ct = default)
    {
        var hold = await _db.Holds.FirstOrDefaultAsync(h => h.Id == holdId, ct);
        if (hold == null)
        {
            throw new NotFoundException($"Hold with ID '{holdId}' was not found.");
        }

        var nowUtc = DateTime.UtcNow;
        var status = hold.Status;

        if (hold.Status == HoldStatus.Held && hold.ExpiresAt <= nowUtc)
        {
            status = HoldStatus.Expired;
            hold.Status = HoldStatus.Expired;
            await _db.SaveChangesAsync(ct);
        }

        return new HoldDetailResponse
        {
            HoldId = hold.Id,
            ClassId = hold.ClassId,
            Status = status,
            ExpiresAt = hold.ExpiresAt.ToString("o")
        };
    }

    public async Task<ConfirmHoldResponse> ConfirmHoldAsync(string holdId, string studentId, CancellationToken ct = default)
    {
        var hold = await _db.Holds.FirstOrDefaultAsync(h => h.Id == holdId, ct);
        if (hold == null)
        {
            throw new NotFoundException($"Hold with ID '{holdId}' was not found.");
        }

        if (!string.Equals(hold.StudentId, studentId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("Student ID does not match the hold owner.");
        }

        // Idempotent confirmation
        if (hold.Status == HoldStatus.Confirmed)
        {
            return new ConfirmHoldResponse
            {
                HoldId = hold.Id,
                Status = HoldStatus.Confirmed
            };
        }

        if (hold.Status == HoldStatus.Cancelled)
        {
            throw new ConflictException("Hold has already been cancelled.");
        }

        var nowUtc = DateTime.UtcNow;
        if (hold.Status == HoldStatus.Expired || (hold.Status == HoldStatus.Held && hold.ExpiresAt <= nowUtc))
        {
            if (hold.Status == HoldStatus.Held)
            {
                hold.Status = HoldStatus.Expired;
                await _db.SaveChangesAsync(ct);
            }
            throw new ConflictException("Hold has expired and cannot be confirmed.");
        }

        hold.Status = HoldStatus.Confirmed;
        hold.ConfirmedAt = nowUtc;
        await _db.SaveChangesAsync(ct);

        return new ConfirmHoldResponse
        {
            HoldId = hold.Id,
            Status = HoldStatus.Confirmed
        };
    }

    public async Task CancelHoldAsync(string holdId, string studentId, CancellationToken ct = default)
    {
        var hold = await _db.Holds.FirstOrDefaultAsync(h => h.Id == holdId, ct);
        if (hold == null)
        {
            throw new NotFoundException($"Hold with ID '{holdId}' was not found.");
        }

        if (!string.Equals(hold.StudentId, studentId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("Student ID does not match the hold owner.");
        }

        if (hold.Status == HoldStatus.Confirmed)
        {
            throw new BadRequestException("Cannot cancel a confirmed admission.");
        }

        if (hold.Status == HoldStatus.Cancelled)
        {
            return; // Idempotent cancellation
        }

        hold.Status = HoldStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StudentHoldResponse>> GetHoldsForStudentAsync(string studentId, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;

        var holds = await _db.Holds
            .AsNoTracking()
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

        return holds.Select(h =>
        {
            var effectiveStatus = (h.Status == HoldStatus.Held && h.ExpiresAt <= nowUtc)
                ? HoldStatus.Expired
                : h.Status;

            return new StudentHoldResponse
            {
                HoldId = h.Id,
                ClassId = h.ClassId,
                ClassName = h.Class?.Name ?? string.Empty,
                Status = effectiveStatus,
                ExpiresAt = h.ExpiresAt.ToString("o")
            };
        }).ToList();
    }
}

