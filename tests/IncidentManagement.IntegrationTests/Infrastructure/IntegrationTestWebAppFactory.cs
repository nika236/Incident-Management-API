using IncidentManagement.Application.Interfaces;
using IncidentManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace IncidentManagement.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=incident_test;Username=incident;Password=incident";

    private PostgreSqlContainer? _postgres;
    private string _connectionString = FallbackConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("Jwt:SigningKey", "integration_test_signing_key_at_least_32_chars");
        builder.UseSetting("SlaMonitor:RunIntervalMinutes", "60");
        builder.UseSetting("Housekeeping:RunIntervalMinutes", "60");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                            || d.ServiceType == typeof(ApplicationDbContext)
                            || d.ServiceType == typeof(DbContextOptions)
                            || d.ServiceType == typeof(IApplicationDbContext))
                .ToList();
            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        });
    }

    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URL") ?? FallbackConnectionString;

        if (string.Equals(Environment.GetEnvironmentVariable("USE_TESTCONTAINERS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            _postgres = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("incident_test")
                .WithUsername("test")
                .WithPassword("test")
                .Build();
            await _postgres.StartAsync();
            _connectionString = _postgres.GetConnectionString();
        }
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
