using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.Interfaces;

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
        
  // Security services
   services.AddSingleton<IRateLimitService, RateLimitService>();
        services.AddSingleton<IInputSanitizer, InputSanitizer>();
   
        // Specialized services
services.AddScoped<ITailorRegistrationService, TailorRegistrationService>();
        
        return services;
    }

    /// <summary>
    /// Adds rate limiting configuration
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, Action<RateLimitOptions>? configure = null)
    {
  var options = new RateLimitOptions();
        configure?.Invoke(options);
      
 services.AddSingleton(options);
        services.AddSingleton<IRateLimitService, RateLimitService>();
        
        return services;
    }
}

public class RateLimitOptions
{
    public int MaxLoginAttempts { get; set; } = 5;
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan SlidingWindow { get; set; } = TimeSpan.FromMinutes(5);
}
