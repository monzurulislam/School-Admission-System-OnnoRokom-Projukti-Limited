using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Domain.Enums;
using SchoolAdmission.Api.Infrastructure.Data;

namespace SchoolAdmission.Api.BackgroundServices;

public class HoldCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HoldCleanupBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public HoldCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<HoldCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hold cleanup background service started.");

        using var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();

                var nowUtc = DateTime.UtcNow;

                var expiredCount = await db.Holds
                    .Where(h => h.Status == HoldStatus.Held && h.ExpiresAt <= nowUtc)
                    .ExecuteUpdateAsync(setter =>
                        setter.SetProperty(h => h.Status, HoldStatus.Expired),
                        stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation("Marked {Count} expired holds as EXPIRED.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up expired holds.");
            }
        }

        _logger.LogInformation("Hold cleanup background service stopped.");
    }
}

