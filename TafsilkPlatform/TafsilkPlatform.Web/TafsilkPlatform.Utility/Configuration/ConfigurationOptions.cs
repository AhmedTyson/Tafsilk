namespace TafsilkPlatform.Utility.Configuration
{
    /// <summary>
    /// Application-wide configuration options
    /// </summary>
    public class ApplicationOptions
    {
        public const string SectionName = "Application";

        public string Name { get; set; } = "Tafsilk Platform";
        public string Version { get; set; } = "1.0.0";
        public string SupportEmail { get; set; } = "support@tafsilk.com";
    }

    /// <summary>
    /// Feature flags configuration
    /// </summary>
    public class FeaturesOptions
    {
        public const string SectionName = "Features";

        public bool EnableGoogleOAuth { get; set; } = true;
        public bool EnableFacebookOAuth { get; set; } = true;
        public bool EnableEmailVerification { get; set; } = true;
        public bool EnableSmsNotifications { get; set; } = false;
        public bool EnableSwagger { get; set; }
        public bool EnableDetailedErrors { get; set; }
    }

    /// <summary>
    /// JWT configuration options
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "TafsilkPlatform";
        public string Audience { get; set; } = "TafsilkPlatformUsers";
        public int AccessTokenExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 30;
    }

    /// <summary>
    /// Email service configuration
    /// </summary>
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "noreply@tafsilk.com";
        public string FromName { get; set; } = "Tafsilk Platform";
        public bool EnableSsl { get; set; } = true;
    }

    /// <summary>
    /// File upload configuration
    /// </summary>
    public class FileUploadOptions
    {
        public const string SectionName = "FileUpload";

        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
        public string[] AllowedImageExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        public string[] AllowedDocumentExtensions { get; set; } = new[] { ".pdf", ".doc", ".docx" };
        public string UploadPath { get; set; } = "wwwroot/uploads";
        public string ProfilePicturesPath { get; set; } = "wwwroot/images/profiles";
        public string PortfolioImagesPath { get; set; } = "wwwroot/images/portfolio";
    }

    /// <summary>
    /// Pagination configuration
    /// </summary>
    public class PaginationOptions
    {
        public const string SectionName = "Pagination";

        public int DefaultPageSize { get; set; } = 20;
        public int MaxPageSize { get; set; } = 100;
    }

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        public bool Enabled { get; set; } = true;
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 10;
    }
}
