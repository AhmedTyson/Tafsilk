using System.Diagnostics;
using System.Text;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with performance metrics
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "Password"
    };

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
  _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files and health checks
        if (IsStaticFile(context.Request.Path) || context.Request.Path.StartsWithSegments("/health"))
     {
            await _next(context);
 return;
   }

        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        // Log request
     await LogRequest(context, requestId);

   // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

    try
   {
            await _next(context);
            stopwatch.Stop();

      // Log response
         await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

// Copy response back to original stream
        await responseBody.CopyToAsync(originalBodyStream);
        }
 finally
        {
            context.Response.Body = originalBodyStream;
  }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
      
    var logMessage = new StringBuilder();
        logMessage.AppendLine($"[{requestId}] HTTP Request:");
  logMessage.AppendLine($"  Method: {request.Method}");
        logMessage.AppendLine($"  Path: {request.Path}{request.QueryString}");
        logMessage.AppendLine($"  User: {context.User.Identity?.Name ?? "Anonymous"}");
        logMessage.AppendLine($"  IP: {context.Connection.RemoteIpAddress}");

        // Log headers (excluding sensitive ones)
     if (request.Headers.Any())
        {
      logMessage.AppendLine("  Headers:");
          foreach (var (key, value) in request.Headers.Where(h => !SensitiveHeaders.Contains(h.Key)))
  {
logMessage.AppendLine($"    {key}: {value}");
            }
        }

        // Log request body for POST/PUT (with size limit)
        if ((request.Method == "POST" || request.Method == "PUT") && request.ContentLength > 0 && request.ContentLength < 1024 * 10)
   {
            request.EnableBuffering();
      var buffer = new byte[Convert.ToInt32(request.ContentLength)];
   await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
  var bodyAsText = Encoding.UTF8.GetString(buffer);
         request.Body.Position = 0;

  // Sanitize sensitive data in body
            var sanitizedBody = SanitizeSensitiveData(bodyAsText);
            logMessage.AppendLine($"  Body: {sanitizedBody}");
        }

        _logger.LogInformation(logMessage.ToString());
    }

    private async Task LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        var response = context.Response;
  
        var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
         response.StatusCode >= 400 ? LogLevel.Warning :
    LogLevel.Information;

 response.Body.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(response.Body).ReadToEndAsync();
      response.Body.Seek(0, SeekOrigin.Begin);

   var logMessage = new StringBuilder();
   logMessage.AppendLine($"[{requestId}] HTTP Response:");
    logMessage.AppendLine($"  Status: {response.StatusCode}");
        logMessage.AppendLine($"  Duration: {elapsedMs}ms");
        
if (response.StatusCode >= 400 && !string.IsNullOrEmpty(bodyText) && bodyText.Length < 1024 * 5)
        {
       logMessage.AppendLine($"  Body: {bodyText}");
        }

        _logger.Log(logLevel, logMessage.ToString());

   // Log slow requests
        if (elapsedMs > 1000)
   {
       _logger.LogWarning($"[{requestId}] Slow request detected: {context.Request.Method} {context.Request.Path} took {elapsedMs}ms");
}
    }

    private static bool IsStaticFile(PathString path)
    {
        var staticExtensions = new[] { ".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot" };
        return staticExtensions.Any(ext => path.Value?.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static string SanitizeSensitiveData(string input)
    {
        // Sanitize password fields
        var patterns = new[]
 {
     @"""password""\s*:\s*""[^""]*""",
         @"""passwordHash""\s*:\s*""[^""]*""",
            @"""token""\s*:\s*""[^""]*""",
        @"""apiKey""\s*:\s*""[^""]*"""
        };

 foreach (var pattern in patterns)
        {
            input = System.Text.RegularExpressions.Regex.Replace(
    input, pattern, m => m.Value.Substring(0, m.Value.IndexOf(':') + 1) + " \"***\"", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
     }

        return input;
}
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
     return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
