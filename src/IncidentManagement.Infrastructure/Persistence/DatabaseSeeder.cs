using IncidentManagement.Application.Interfaces;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IncidentManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ResponderUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ViewerUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task SeedAsync(IApplicationDbContext db, IPasswordHasher passwordHasher, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding development users");

        var users = new[]
        {
            new User
            {
                Id = AdminUserId,
                Email = "admin@incident.local",
                DisplayName = "NOC Admin",
                Role = UserRole.Admin,
                PasswordHash = passwordHasher.Hash("Admin123!")
            },
            new User
            {
                Id = ResponderUserId,
                Email = "responder@incident.local",
                DisplayName = "On-Call Responder",
                Role = UserRole.Responder,
                PasswordHash = passwordHasher.Hash("Responder123!")
            },
            new User
            {
                Id = ViewerUserId,
                Email = "viewer@incident.local",
                DisplayName = "Operations Viewer",
                Role = UserRole.Viewer,
                PasswordHash = passwordHasher.Hash("Viewer123!")
            }
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync(cancellationToken);
    }
}
