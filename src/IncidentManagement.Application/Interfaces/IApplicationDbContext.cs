using IncidentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentComment> IncidentComments { get; }
    DbSet<IncidentStatusAudit> IncidentStatusAudits { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
