using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Contracts.Requests;
using SchoolAdmission.Api.Contracts.Responses;
using SchoolAdmission.Api.Domain.Entities;
using SchoolAdmission.Api.Domain.Enums;
using SchoolAdmission.Api.Domain.Exceptions;
using SchoolAdmission.Api.Infrastructure.Data;

namespace SchoolAdmission.Api.Services;

public class ClassService : IClassService
{
    private readonly AdmissionDbContext _db;

    public ClassService(AdmissionDbContext db)
    {
        _db = db;
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Class name is required.");
        }

        if (request.TotalSeats <= 0)
        {
            throw new BadRequestException("totalSeats must be greater than 0.");
        }

        var trimmedName = request.Name.Trim();
        var exists = await _db.Classes
            .AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower(), ct);

        if (exists)
        {
            throw new ConflictException($"A class with the name '{trimmedName}' already exists.");
        }

        var newClass = new Class
        {
            Name = trimmedName,
            TotalSeats = request.TotalSeats,
            CreatedAt = DateTime.UtcNow
        };

        _db.Classes.Add(newClass);
        await _db.SaveChangesAsync(ct);

        return new ClassResponse
        {
            Id = newClass.Id,
            Name = newClass.Name,
            TotalSeats = newClass.TotalSeats
        };
    }

    public async Task<IReadOnlyList<ClassDetailResponse>> GetAllClassesAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;

        var classes = await _db.Classes
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalSeats,
                HeldSeats = c.Holds.Count(h => h.Status == HoldStatus.Held && h.ExpiresAt > nowUtc),
                ConfirmedSeats = c.Holds.Count(h => h.Status == HoldStatus.Confirmed)
            })
            .ToListAsync(ct);

        return classes.Select(c => new ClassDetailResponse
        {
            Id = c.Id,
            Name = c.Name,
            TotalSeats = c.TotalSeats,
            HeldSeats = c.HeldSeats,
            ConfirmedSeats = c.ConfirmedSeats,
            AvailableSeats = Math.Max(0, c.TotalSeats - (c.HeldSeats + c.ConfirmedSeats))
        }).ToList();
    }

    public async Task<ClassDetailResponse> GetClassByIdAsync(string classId, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;

        var classItem = await _db.Classes
            .AsNoTracking()
            .Where(c => c.Id == classId)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalSeats,
                HeldSeats = c.Holds.Count(h => h.Status == HoldStatus.Held && h.ExpiresAt > nowUtc),
                ConfirmedSeats = c.Holds.Count(h => h.Status == HoldStatus.Confirmed)
            })
            .FirstOrDefaultAsync(ct);

        if (classItem == null)
        {
            throw new NotFoundException($"Class with ID '{classId}' was not found.");
        }

        return new ClassDetailResponse
        {
            Id = classItem.Id,
            Name = classItem.Name,
            TotalSeats = classItem.TotalSeats,
            HeldSeats = classItem.HeldSeats,
            ConfirmedSeats = classItem.ConfirmedSeats,
            AvailableSeats = Math.Max(0, classItem.TotalSeats - (classItem.HeldSeats + classItem.ConfirmedSeats))
        };
    }
}

