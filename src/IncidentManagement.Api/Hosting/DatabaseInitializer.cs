using IncidentManagement.Application.Interfaces;
using IncidentManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Api.Hosting;

public static class DatabaseInitializer
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync(cancellationToken);
                await DatabaseSeeder.SeedAsync(
                    db,
                    scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
                    scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder"),
                    cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
            {
                var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max})", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        throw new InvalidOperationException("Database migration/seed failed after multiple attempts.");
    }
}
