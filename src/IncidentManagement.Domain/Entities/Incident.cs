using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public Guid? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime AcknowledgeSlaTarget { get; set; }
    public DateTime ResolveSlaTarget { get; set; }
    public bool IsAcknowledgeSlaBreached { get; set; }
    public bool IsResolveSlaBreached { get; set; }
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedAt { get; set; }

    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
    public ICollection<IncidentStatusAudit> StatusAudits { get; set; } = new List<IncidentStatusAudit>();
}
