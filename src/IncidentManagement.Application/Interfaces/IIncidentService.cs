using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Interfaces;

public interface IIncidentService
{
    Task<PagedResult<IncidentDto>> ListAsync(IncidentQuery query, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> UpdateAsync(Guid id, UpdateIncidentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> TransitionStatusAsync(Guid id, TransitionStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IncidentCommentDto> AddCommentAsync(Guid id, AddCommentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
}
