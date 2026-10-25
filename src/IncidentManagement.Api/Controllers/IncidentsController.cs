using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Authorization.RolePolicies.AnyAuthenticated)]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly ICurrentUserService _currentUser;

    public IncidentsController(IIncidentService incidentService, ICurrentUserService currentUser)
    {
        _incidentService = incidentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = Authorization.RolePolicies.AnyAuthenticated)]
    [ProducesResponseType(typeof(PagedResult<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IncidentDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] IncidentStatus? status = null,
        [FromQuery] IncidentSeverity? severity = null,
        [FromQuery] Guid? assigneeId = null,
        [FromQuery] bool? isEscalated = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new IncidentQuery(page, pageSize, sortBy, sortDirection, status, severity, assigneeId, isEscalated, search);
        var result = await _incidentService.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Authorization.RolePolicies.AnyAuthenticated)]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.GetByIdAsync(id, cancellationToken);
        return Ok(incident);
    }

    [HttpPost]
    [Authorize(Policy = Authorization.RolePolicies.ResponderOrAdmin)]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<IncidentDetailDto>> Create([FromBody] CreateIncidentRequest request, CancellationToken cancellationToken)
    {
        var actorId = RequireUserId();
        var incident = await _incidentService.CreateAsync(request, actorId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = incident.Id }, incident);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Authorization.RolePolicies.ResponderOrAdmin)]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Update(Guid id, [FromBody] UpdateIncidentRequest request, CancellationToken cancellationToken)
    {
        var actorId = RequireUserId();
        var incident = await _incidentService.UpdateAsync(id, request, actorId, cancellationToken);
        return Ok(incident);
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = Authorization.RolePolicies.ResponderOrAdmin)]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> TransitionStatus(
        Guid id,
        [FromBody] TransitionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = RequireUserId();
        var incident = await _incidentService.TransitionStatusAsync(id, request, actorId, cancellationToken);
        return Ok(incident);
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize(Policy = Authorization.RolePolicies.ResponderOrAdmin)]
    [ProducesResponseType(typeof(IncidentCommentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<IncidentCommentDto>> AddComment(
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = RequireUserId();
        var comment = await _incidentService.AddCommentAsync(id, request, actorId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, comment);
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new ForbiddenException("User context is missing.");
}
