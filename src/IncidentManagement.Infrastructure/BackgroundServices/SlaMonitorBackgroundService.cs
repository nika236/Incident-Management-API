using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Options;
using IncidentManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentManagement.Infrastructure.BackgroundServices;

public class SlaMonitorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaMonitorBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public SlaMonitorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SlaMonitorBackgroundService> logger,
        IOptions<SlaMonitorOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.RunIntervalMinutes));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA monitor started (interval {Interval})", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "SLA monitor iteration failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;

        var openIncidents = await db.Incidents
            .Where(i => i.Status != IncidentStatus.Closed && i.Status != IncidentStatus.Resolved)
            .ToListAsync(cancellationToken);

        var changed = 0;
        foreach (var incident in openIncidents)
        {
            var ackBreached = incident.AcknowledgedAt is null && now > incident.AcknowledgeSlaTarget;
            var resolveBreached = incident.ResolvedAt is null && now > incident.ResolveSlaTarget;

            if (ackBreached && !incident.IsAcknowledgeSlaBreached)
            {
                incident.IsAcknowledgeSlaBreached = true;
                incident.IsEscalated = true;
                incident.EscalatedAt = now;
                changed++;
            }

            if (resolveBreached && !incident.IsResolveSlaBreached)
            {
                incident.IsResolveSlaBreached = true;
                if (!incident.IsEscalated)
                {
                    incident.IsEscalated = true;
                    incident.EscalatedAt = now;
                }
                changed++;
            }
        }

        if (changed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("SLA monitor flagged {Count} incident breach(es)", changed);
        }
    }
}
