using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Services;

public static class IncidentStatusTransitionValidator
{
    private static readonly Dictionary<IncidentStatus, HashSet<IncidentStatus>> AllowedTransitions = new()
    {
        [IncidentStatus.Open] = [IncidentStatus.Acknowledged, IncidentStatus.Investigating, IncidentStatus.Closed],
        [IncidentStatus.Acknowledged] = [IncidentStatus.Investigating, IncidentStatus.Mitigated, IncidentStatus.Resolved, IncidentStatus.Closed],
        [IncidentStatus.Investigating] = [IncidentStatus.Mitigated, IncidentStatus.Resolved, IncidentStatus.Closed],
        [IncidentStatus.Mitigated] = [IncidentStatus.Resolved, IncidentStatus.Investigating, IncidentStatus.Closed],
        [IncidentStatus.Resolved] = [IncidentStatus.Closed, IncidentStatus.Investigating],
        [IncidentStatus.Closed] = []
    };

    public static bool CanTransition(IncidentStatus from, IncidentStatus to) =>
        AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);

    public static IReadOnlyCollection<IncidentStatus> GetAllowedTransitions(IncidentStatus from) =>
        AllowedTransitions.TryGetValue(from, out var targets)
            ? targets
            : Array.Empty<IncidentStatus>();
}
