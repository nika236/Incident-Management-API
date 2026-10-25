using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Options;
using IncidentManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentManagement.Infrastructure.BackgroundServices;

public class HousekeepingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HousekeepingBackgroundService> _logger;
    private readonly HousekeepingOptions _options;
    private readonly TimeSpan _interval;

    public HousekeepingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<HousekeepingBackgroundService> logger,
        IOptions<HousekeepingOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _interval = TimeSpan.FromMinutes(Math.Max(1, _options.RunIntervalMinutes));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Housekeeping service started (interval {Interval})", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AutoCloseResolvedAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Housekeeping iteration failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task AutoCloseResolvedAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-_options.AutoCloseResolvedAfterDays);

        var stale = await db.Incidents
            .Where(i => i.Status == IncidentStatus.Resolved && i.ResolvedAt != null && i.ResolvedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (stale.Count == 0)
            return;

        var systemUserId = await db.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (systemUserId == Guid.Empty)
            return;

        var now = DateTime.UtcNow;
        foreach (var incident in stale)
        {
            var from = incident.Status;
            incident.Status = IncidentStatus.Closed;
            incident.ClosedAt = now;
            db.IncidentStatusAudits.Add(new IncidentStatusAudit
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                FromStatus = from,
                ToStatus = IncidentStatus.Closed,
                ChangedByUserId = systemUserId,
                Note = "Auto-closed by housekeeping after resolved retention period.",
                ChangedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Housekeeping auto-closed {Count} resolved incident(s)", stale.Count);
    }
}
