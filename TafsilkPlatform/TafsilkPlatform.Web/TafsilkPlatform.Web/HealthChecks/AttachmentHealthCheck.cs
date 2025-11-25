using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TafsilkPlatform.Web.HealthChecks
{
    public class AttachmentHealthCheck : IHealthCheck
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttachmentHealthCheck> _logger;
        private readonly IConfiguration _configuration;

        public AttachmentHealthCheck(IWebHostEnvironment env, ILogger<AttachmentHealthCheck> logger, IConfiguration configuration)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                    ? Path.Combine(_env.ContentRootPath, "wwwroot")
                    : _env.WebRootPath;

                var attachmentsPath = Path.Combine(webRoot, "Attachments");

                // Ensure directory exists
                Directory.CreateDirectory(attachmentsPath);

                // Test write access by creating and deleting a small temp file
                var testFileName = Path.Combine(attachmentsPath, $"hc_{Guid.NewGuid()}.tmp");
                var testData = new byte[] { 0x1 };
                await File.WriteAllBytesAsync(testFileName, testData, cancellationToken);
                File.Delete(testFileName);

                // Check free space on drive
                var root = Path.GetPathRoot(attachmentsPath) ?? Path.GetPathRoot(_env.ContentRootPath);
                if (string.IsNullOrEmpty(root))
                {
                    _logger.LogWarning("Attachment health check could not determine drive root");
                    return HealthCheckResult.Unhealthy("Cannot determine drive root for attachments");
                }

                var drive = new DriveInfo(root);
                var freeBytes = drive.AvailableFreeSpace;

                // Minimum free bytes - configurable (bytes). Defaults to 100 MB.
                var minFreeBytes = _configuration.GetValue<long?>("HealthChecks:MinFreeDiskBytes") ?? 100L * 1024 * 1024;

                if (freeBytes < minFreeBytes)
                {
                    var msg = $"Low disk space on drive {drive.Name}. Available={freeBytes} bytes, Required>={minFreeBytes} bytes";
                    _logger.LogWarning(msg);
                    var data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "availableBytes", freeBytes },
                        { "requiredBytes", minFreeBytes }
                    };
                    return HealthCheckResult.Unhealthy(description: msg, data: data);
                }

                var healthyData = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "availableBytes", freeBytes }
                };

                return HealthCheckResult.Healthy(description: "Attachments writable and disk space OK", data: healthyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attachment health check failed");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
