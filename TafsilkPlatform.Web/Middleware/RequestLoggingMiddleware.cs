using System.Diagnostics;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Middleware to log request/response details for debugging and monitoring
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files
        if (context.Request.Path.StartsWithSegments("/css") ||
     context.Request.Path.StartsWithSegments("/js") ||
       context.Request.Path.StartsWithSegments("/lib") ||
   context.Request.Path.StartsWithSegments("/images"))
        {
       await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
      
        // Log request
        _logger.LogInformation(
       "[{RequestId}] {Method} {Path} - User: {User}",
   requestId,
  context.Request.Method,
       context.Request.Path,
       context.User.Identity?.Name ?? "Anonymous");

        try
        {
       await _next(context);
  }
        finally
     {
   stopwatch.Stop();
            
   var level = context.Response.StatusCode >= 500 
 ? LogLevel.Error 
       : context.Response.StatusCode >= 400 
    ? LogLevel.Warning 
         : LogLevel.Information;

    _logger.Log(level,
         "[{RequestId}] {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
       requestId,
       context.Request.Method,
            context.Request.Path,
        context.Response.StatusCode,
       stopwatch.ElapsedMilliseconds);
   }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
