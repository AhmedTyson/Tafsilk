using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all custom services for the application
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IValidationService, ValidationService>();

        // Specialized services
        services.AddScoped<ITailorRegistrationService, TailorRegistrationService>();

        return services;
    }
}
