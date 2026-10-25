using FluentValidation;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        return services;
    }
}
