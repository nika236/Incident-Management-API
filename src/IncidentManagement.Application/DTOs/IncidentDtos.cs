using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public record CreateIncidentRequest(string Title, string Description, IncidentSeverity Severity, Guid? AssigneeId);

public record UpdateIncidentRequest(string? Title, string? Description, IncidentSeverity? Severity, Guid? AssigneeId);

public record TransitionStatusRequest(IncidentStatus Status, string? Note);

public record AddCommentRequest(string Body);

public record IncidentDto(
    Guid Id,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    Guid? AssigneeId,
    string? AssigneeDisplayName,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    DateTime AcknowledgeSlaTarget,
    DateTime ResolveSlaTarget,
    bool IsAcknowledgeSlaBreached,
    bool IsResolveSlaBreached,
    bool IsEscalated,
    DateTime? EscalatedAt);

public record IncidentDetailDto(
    Guid Id,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    Guid? AssigneeId,
    string? AssigneeDisplayName,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    DateTime AcknowledgeSlaTarget,
    DateTime ResolveSlaTarget,
    bool IsAcknowledgeSlaBreached,
    bool IsResolveSlaBreached,
    bool IsEscalated,
    DateTime? EscalatedAt,
    IReadOnlyList<IncidentStatus> AllowedTransitions,
    IReadOnlyList<IncidentCommentDto> Comments,
    IReadOnlyList<IncidentStatusAuditDto> StatusAudits);

public record IncidentCommentDto(Guid Id, Guid AuthorId, string AuthorDisplayName, string Body, DateTime CreatedAt);

public record IncidentStatusAuditDto(
    Guid Id,
    IncidentStatus FromStatus,
    IncidentStatus ToStatus,
    Guid ChangedByUserId,
    string ChangedByDisplayName,
    string? Note,
    DateTime ChangedAt);

public record IncidentQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "createdAt",
    string? SortDirection = "desc",
    IncidentStatus? Status = null,
    IncidentSeverity? Severity = null,
    Guid? AssigneeId = null,
    bool? IsEscalated = null,
    string? Search = null);
