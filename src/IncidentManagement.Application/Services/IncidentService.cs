using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Application.Mapping;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly IApplicationDbContext _db;

    public IncidentService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<IncidentDto>> ListAsync(IncidentQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<Incident> q = _db.Incidents.AsNoTracking().Include(i => i.Assignee);

        if (query.Status.HasValue)
            q = q.Where(i => i.Status == query.Status.Value);
        if (query.Severity.HasValue)
            q = q.Where(i => i.Severity == query.Severity.Value);
        if (query.AssigneeId.HasValue)
            q = q.Where(i => i.AssigneeId == query.AssigneeId.Value);
        if (query.IsEscalated.HasValue)
            q = q.Where(i => i.IsEscalated == query.IsEscalated.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(i => i.Title.ToLower().Contains(term) || i.Description.ToLower().Contains(term));
        }

        q = ApplySorting(q, query.SortBy, query.SortDirection);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<IncidentDto>
        {
            Items = items.Select(IncidentMapper.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await LoadDetailQuery().FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Incident {id} was not found.");
        return IncidentMapper.ToDetailDto(incident);
    }

    public async Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (request.AssigneeId.HasValue)
            await EnsureUserExists(request.AssigneeId.Value, cancellationToken);

        var createdAt = DateTime.UtcNow;
        var (ackTarget, resolveTarget) = SlaCalculator.ComputeTargets(request.Severity, createdAt);

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity,
            Status = IncidentStatus.Open,
            AssigneeId = request.AssigneeId,
            CreatedAt = createdAt,
            AcknowledgeSlaTarget = ackTarget,
            ResolveSlaTarget = resolveTarget
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(incident.Id, cancellationToken);
    }

    public async Task<IncidentDetailDto> UpdateAsync(Guid id, UpdateIncidentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Incident {id} was not found.");

        if (incident.Status == IncidentStatus.Closed)
            throw new AppException("Closed incidents cannot be updated.");

        if (request.Title is not null)
            incident.Title = request.Title.Trim();
        if (request.Description is not null)
            incident.Description = request.Description.Trim();
        if (request.AssigneeId.HasValue)
        {
            await EnsureUserExists(request.AssigneeId.Value, cancellationToken);
            incident.AssigneeId = request.AssigneeId;
        }

        if (request.Severity.HasValue && request.Severity != incident.Severity)
        {
            incident.Severity = request.Severity.Value;
            var (ackTarget, resolveTarget) = SlaCalculator.ComputeTargets(incident.Severity, incident.CreatedAt);
            incident.AcknowledgeSlaTarget = ackTarget;
            incident.ResolveSlaTarget = resolveTarget;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<IncidentDetailDto> TransitionStatusAsync(
        Guid id,
        TransitionStatusRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Incident {id} was not found.");

        var from = incident.Status;
        var to = request.Status;

        if (from == to)
            throw new AppException($"Incident is already in status {to}.");

        if (!IncidentStatusTransitionValidator.CanTransition(from, to))
            throw new AppException($"Transition from {from} to {to} is not allowed.");

        var now = DateTime.UtcNow;
        incident.Status = to;

        if (to == IncidentStatus.Acknowledged && incident.AcknowledgedAt is null)
            incident.AcknowledgedAt = now;
        if (to == IncidentStatus.Resolved)
            incident.ResolvedAt = now;
        if (to == IncidentStatus.Closed)
            incident.ClosedAt = now;

        _db.IncidentStatusAudits.Add(new IncidentStatusAudit
        {
            Id = Guid.NewGuid(),
            IncidentId = incident.Id,
            FromStatus = from,
            ToStatus = to,
            ChangedByUserId = actorUserId,
            Note = request.Note?.Trim(),
            ChangedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<IncidentCommentDto> AddCommentAsync(
        Guid id,
        AddCommentRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.Incidents.AnyAsync(i => i.Id == id, cancellationToken);
        if (!exists)
            throw new NotFoundException($"Incident {id} was not found.");

        var author = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken)
            ?? throw new NotFoundException("Current user was not found.");

        var comment = new IncidentComment
        {
            Id = Guid.NewGuid(),
            IncidentId = id,
            AuthorId = actorUserId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.IncidentComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        return new IncidentCommentDto(comment.Id, author.Id, author.DisplayName, comment.Body, comment.CreatedAt);
    }

    private IQueryable<Incident> LoadDetailQuery() =>
        _db.Incidents.AsNoTracking()
            .Include(i => i.Assignee)
            .Include(i => i.Comments).ThenInclude(c => c.Author)
            .Include(i => i.StatusAudits).ThenInclude(a => a.ChangedByUser);

    private static IQueryable<Incident> ApplySorting(IQueryable<Incident> q, string? sortBy, string? direction)
    {
        var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => desc ? q.OrderByDescending(i => i.Title) : q.OrderBy(i => i.Title),
            "severity" => desc ? q.OrderByDescending(i => i.Severity) : q.OrderBy(i => i.Severity),
            "status" => desc ? q.OrderByDescending(i => i.Status) : q.OrderBy(i => i.Status),
            "acknowledgeslatarget" => desc ? q.OrderByDescending(i => i.AcknowledgeSlaTarget) : q.OrderBy(i => i.AcknowledgeSlaTarget),
            _ => desc ? q.OrderByDescending(i => i.CreatedAt) : q.OrderBy(i => i.CreatedAt)
        };
    }

    private async Task EnsureUserExists(Guid userId, CancellationToken cancellationToken)
    {
        var ok = await _db.Users.AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (!ok)
            throw new AppException($"User {userId} was not found or is inactive.");
    }
}
