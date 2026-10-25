using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Services;

public static class SlaCalculator
{
    public static (TimeSpan AcknowledgeWindow, TimeSpan ResolveWindow) GetWindows(IncidentSeverity severity) =>
        severity switch
        {
            IncidentSeverity.SEV1 => (TimeSpan.FromMinutes(15), TimeSpan.FromHours(4)),
            IncidentSeverity.SEV2 => (TimeSpan.FromMinutes(30), TimeSpan.FromHours(8)),
            IncidentSeverity.SEV3 => (TimeSpan.FromHours(2), TimeSpan.FromHours(24)),
            IncidentSeverity.SEV4 => (TimeSpan.FromHours(8), TimeSpan.FromHours(72)),
            _ => throw new ArgumentOutOfRangeException(nameof(severity))
        };

    public static (DateTime AcknowledgeTarget, DateTime ResolveTarget) ComputeTargets(
        IncidentSeverity severity,
        DateTime createdAt)
    {
        var (ack, resolve) = GetWindows(severity);
        return (createdAt.Add(ack), createdAt.Add(resolve));
    }
}
