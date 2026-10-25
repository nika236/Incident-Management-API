using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Services;

namespace IncidentManagement.Application.Mapping;

public static class IncidentMapper
{
    public static IncidentDto ToDto(Incident incident) => new(
        incident.Id,
        incident.Title,
        incident.Description,
        incident.Severity,
        incident.Status,
        incident.AssigneeId,
        incident.Assignee?.DisplayName,
        incident.CreatedAt,
        incident.AcknowledgedAt,
        incident.ResolvedAt,
        incident.ClosedAt,
        incident.AcknowledgeSlaTarget,
        incident.ResolveSlaTarget,
        incident.IsAcknowledgeSlaBreached,
        incident.IsResolveSlaBreached,
        incident.IsEscalated,
        incident.EscalatedAt);

    public static IncidentDetailDto ToDetailDto(Incident incident) => new(
        incident.Id,
        incident.Title,
        incident.Description,
        incident.Severity,
        incident.Status,
        incident.AssigneeId,
        incident.Assignee?.DisplayName,
        incident.CreatedAt,
        incident.AcknowledgedAt,
        incident.ResolvedAt,
        incident.ClosedAt,
        incident.AcknowledgeSlaTarget,
        incident.ResolveSlaTarget,
        incident.IsAcknowledgeSlaBreached,
        incident.IsResolveSlaBreached,
        incident.IsEscalated,
        incident.EscalatedAt,
        IncidentStatusTransitionValidator.GetAllowedTransitions(incident.Status).ToList(),
        incident.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new IncidentCommentDto(c.Id, c.AuthorId, c.Author.DisplayName, c.Body, c.CreatedAt))
            .ToList(),
        incident.StatusAudits
            .OrderBy(a => a.ChangedAt)
            .Select(a => new IncidentStatusAuditDto(
                a.Id, a.FromStatus, a.ToStatus, a.ChangedByUserId, a.ChangedByUser.DisplayName, a.Note, a.ChangedAt))
            .ToList());
}
