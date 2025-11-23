using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TafsilkPlatform.Utility.Configuration;

/// <summary>
/// Strongly-typed application settings
/// </summary>
public class AppSettings
{
    public JwtSettings Jwt { get; set; } = new();
    public ApplicationSettings Application { get; set; } = new();
    public FeaturesSettings Features { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public FileUploadSettings FileUpload { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public PerformanceSettings Performance { get; set; } = new();
}

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class ApplicationSettings
{
    public string Name { get; set; } = "Tafsilk Platform";
    public string Version { get; set; } = "1.0.0";
    public string SupportEmail { get; set; } = "support@tafsilk.com";
    public string BaseUrl { get; set; } = "https://localhost:5001";
}

public class FeaturesSettings
{
    public bool EnableGoogleOAuth { get; set; } = true;
    public bool EnableFacebookOAuth { get; set; } = true;
    public bool EnableEmailVerification { get; set; } = true;
    public bool EnableSmsNotifications { get; set; } = false;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseCaching { get; set; } = true;
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Tafsilk Platform";
    public bool EnableSsl { get; set; } = true;
}

public class FileUploadSettings
{
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public long MaxImageSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
    public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    public string[] AllowedDocumentExtensions { get; set; } = { ".pdf", ".doc", ".docx" };
    public string UploadPath { get; set; } = "wwwroot/uploads";
}

public class SecuritySettings
{
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int PasswordResetTokenExpirationHours { get; set; } = 1;
    public int EmailVerificationTokenExpirationHours { get; set; } = 24;
    public bool RequireEmailVerification { get; set; } = true;
    public bool RequireTwoFactorForAdmin { get; set; } = false;
}

public class PerformanceSettings
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public int CacheDurationMinutes { get; set; } = 30;
    public bool EnableResponseCompression { get; set; } = true;
    public bool EnableQuerySplitting { get; set; } = true;
}

/// <summary>
/// Extension methods for configuration
/// </summary>
public static class ConfigurationExtensions
{
    public static AppSettings GetAppSettings(this IConfiguration configuration)
    {
        var settings = new AppSettings();

        configuration.GetSection("Jwt").Bind(settings.Jwt);
        configuration.GetSection("Application").Bind(settings.Application);
        configuration.GetSection("Features").Bind(settings.Features);
        configuration.GetSection("Email").Bind(settings.Email);
        configuration.GetSection("FileUpload").Bind(settings.FileUpload);
        configuration.GetSection("Security").Bind(settings.Security);
        configuration.GetSection("Performance").Bind(settings.Performance);

        return settings;
    }

    public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(options => configuration.GetSection("Jwt").Bind(options));
        services.Configure<ApplicationSettings>(options => configuration.GetSection("Application").Bind(options));
        services.Configure<FeaturesSettings>(options => configuration.GetSection("Features").Bind(options));
        services.Configure<EmailSettings>(options => configuration.GetSection("Email").Bind(options));
        services.Configure<FileUploadSettings>(options => configuration.GetSection("FileUpload").Bind(options));
        services.Configure<SecuritySettings>(options => configuration.GetSection("Security").Bind(options));
        services.Configure<PerformanceSettings>(options => configuration.GetSection("Performance").Bind(options));

        return services;
    }
}
