using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text;

namespace TafsilkPlatform.Web.HealthChecks;

/// <summary>
/// Custom health check response writer
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
   context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonSerializerOptions
{
   PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

     var response = new
        {
  status = healthReport.Status.ToString(),
   totalDuration = healthReport.TotalDuration.TotalMilliseconds,
          checks = healthReport.Entries.Select(e => new
        {
      name = e.Key,
      status = e.Value.Status.ToString(),
     description = e.Value.Description,
     duration = e.Value.Duration.TotalMilliseconds,
    exception = e.Value.Exception?.Message,
      data = e.Value.Data
            })
  };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Database health check
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
  }

    public async Task<HealthCheckResult> CheckHealthAsync(
     HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
  
 await dbContext.Database.CanConnectAsync(cancellationToken);
       
      return HealthCheckResult.Healthy("Database connection is healthy");
        }
     catch (Exception ex)
   {
return HealthCheckResult.Unhealthy("Database connection failed", ex);
     }
    }
}

/// <summary>
/// Memory health check
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _threshold;

 public MemoryHealthCheck(long threshold = 1024L * 1024L * 1024L) // 1GB default
{
        _threshold = threshold;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
  var allocated = GC.GetTotalMemory(forceFullCollection: false);
      var data = new Dictionary<string, object>
   {
            { "AllocatedBytes", allocated },
{ "ThresholdBytes", _threshold },
    { "Gen0Collections", GC.CollectionCount(0) },
    { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
     };

        var status = allocated < _threshold 
        ? HealthStatus.Healthy 
        : HealthStatus.Degraded;

     return Task.FromResult(new HealthCheckResult(
            status,
      $"Memory usage: {allocated / (1024 * 1024)}MB",
 data: data));
    }
}

/// <summary>
/// Extensions for health check configuration
/// </summary>
public static class HealthCheckExtensions
{
public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
    {
    services.AddHealthChecks()
       .AddCheck<DatabaseHealthCheck>(
      "database",
     failureStatus: HealthStatus.Unhealthy,
         tags: new[] { "db", "sql", "ready" })
            .AddCheck<MemoryHealthCheck>(
       "memory",
     failureStatus: HealthStatus.Degraded,
  tags: new[] { "memory", "live" })
        .AddCheck("self",
      () => HealthCheckResult.Healthy("Application is running"),
            tags: new[] { "live" });

        return services;
    }

    public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Liveness probe - quick check
        endpoints.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
      {
  Predicate = check => check.Tags.Contains("live"),
ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Readiness probe - includes dependencies
      endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
   {
  Predicate = check => check.Tags.Contains("ready"),
   ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Full health check
     endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
       ResponseWriter = HealthCheckResponseWriter.WriteResponse
     });

return endpoints;
  }
}
