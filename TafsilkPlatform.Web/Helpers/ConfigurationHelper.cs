using Microsoft.Extensions.Configuration;

namespace TafsilkPlatform.Web.Helpers;

/// <summary>
/// Simple helper methods for configuration - makes setup easier
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// Gets a configuration value with a clear error message if missing
    /// </summary>
    public static string GetRequiredValue(this IConfiguration config, string key, string friendlyName)
    {
        var value = config[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"{friendlyName} is not configured. " +
                $"Set it using: dotnet user-secrets set \"{key}\" \"your-value\" " +
                $"(development) or environment variable (production)");
        }
        return value;
    }

    /// <summary>
    /// Gets a configuration value with a default and warning
    /// </summary>
    public static string GetValueWithDefault(this IConfiguration config, string key, string defaultValue, string friendlyName)
    {
        var value = config[key];
        if (string.IsNullOrEmpty(value) || value == defaultValue)
        {
            // Log warning if using default
            return defaultValue;
        }
        return value;
    }

    /// <summary>
    /// Validates that required configuration is present
    /// </summary>
    public static void ValidateRequiredConfiguration(IConfiguration config, ILogger logger)
    {
        var issues = new List<string>();

        // Check JWT Key
        var jwtKey = config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(jwtKey))
        {
            issues.Add("JWT Key is missing. Run: dotnet user-secrets set \"Jwt:Key\" \"YourKeyHere\"");
        }
        else if (jwtKey.Length < 32)
        {
            issues.Add("JWT Key must be at least 32 characters long");
        }

        // Check Connection String
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            issues.Add("Database connection string is missing");
        }

        if (issues.Any())
        {
            logger.LogError("Configuration Issues Found:");
            foreach (var issue in issues)
            {
                logger.LogError("  - {Issue}", issue);
            }
            throw new InvalidOperationException(
                "Configuration is incomplete. Please fix the issues above.");
        }

        logger.LogInformation("✓ Configuration validated successfully");
    }
}

