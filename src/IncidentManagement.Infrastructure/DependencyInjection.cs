using IncidentManagement.Application.Interfaces;
using IncidentManagement.Infrastructure.BackgroundServices;
using IncidentManagement.Infrastructure.Options;
using IncidentManagement.Infrastructure.Persistence;
using IncidentManagement.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<HousekeepingOptions>(configuration.GetSection(HousekeepingOptions.SectionName));
        services.Configure<SlaMonitorOptions>(configuration.GetSection(SlaMonitorOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddHostedService<SlaMonitorBackgroundService>();
        services.AddHostedService<HousekeepingBackgroundService>();

        return services;
    }
}
