using Microsoft.AspNetCore.Diagnostics;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Global exception handler to catch all unhandled exceptions
/// Helps diagnose crashes by logging detailed error information
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 🔴 BREAKPOINT: Uncomment next line to catch ALL unhandled exceptions during debugging
#if DEBUG
        // Debugger.Break();
#endif

        _logger.LogError(exception,
            "🚨 UNHANDLED EXCEPTION 🚨\n" +
            "Exception Type: {ExceptionType}\n" +
            "Message: {Message}\n" +
            "Path: {Path}\n" +
            "Method: {Method}\n" +
            "User: {User}\n" +
            "Stack Trace:\n{StackTrace}",
            exception.GetType().Name,
            exception.Message,
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.User?.Identity?.Name ?? "Anonymous",
            exception.StackTrace);

        // Check for specific exception types
        var errorMessage = "An unexpected error occurred";
        var statusCode = StatusCodes.Status500InternalServerError;

        switch (exception)
        {
            case OutOfMemoryException oomEx:
                _logger.LogCritical(oomEx, "OUT OF MEMORY EXCEPTION!");
                errorMessage = "File is too large. Please choose a smaller file";
                statusCode = StatusCodes.Status413RequestEntityTooLarge;
                break;

            case InvalidOperationException ioEx:
                _logger.LogError(ioEx, "Invalid operation exception");
                errorMessage = ioEx.Message;
                statusCode = StatusCodes.Status400BadRequest;
                break;

            case ArgumentException argEx:
                _logger.LogError(argEx, "Argument exception");
                errorMessage = argEx.Message;
                statusCode = StatusCodes.Status400BadRequest;
                break;

            case UnauthorizedAccessException uaEx:
                _logger.LogWarning(uaEx, "Unauthorized access attempt");
                errorMessage = "You are not authorized to perform this action";
                statusCode = StatusCodes.Status403Forbidden;
                break;

            case FileNotFoundException fnfEx:
                _logger.LogError(fnfEx, "File not found");
                errorMessage = "File not found";
                statusCode = StatusCodes.Status404NotFound;
                break;

            case IOException ioException:
                _logger.LogError(ioException, "IO exception");
                errorMessage = "An error occurred while reading or writing the file";
                statusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        // Log memory status
        var gcInfo = GC.GetGCMemoryInfo();
        _logger.LogInformation(
            "Memory Status:\n" +
            "  Total Memory: {TotalMemory:N0} bytes\n" +
            "  Heap Size: {HeapSize:N0} bytes\n" +
            "  Available Memory: {Available:N0} bytes\n" +
            "  High Memory Load: {HighMemoryLoad}",
            gcInfo.TotalAvailableMemoryBytes,
            gcInfo.HeapSizeBytes,
            gcInfo.TotalAvailableMemoryBytes - gcInfo.HeapSizeBytes,
            gcInfo.MemoryLoadBytes > (gcInfo.TotalAvailableMemoryBytes * 0.8));

        httpContext.Response.StatusCode = statusCode;

        // Return JSON for API calls, HTML for regular pages
        if (httpContext.Request.Path.StartsWithSegments("/api"))
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(new
            {
                error = errorMessage,
                type = exception.GetType().Name,
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            // For regular web requests, redirect to error page with message
            httpContext.Response.Redirect($"/Error?message={Uri.EscapeDataString(errorMessage)}");
        }

        return true; // Exception handled
    }
}

/// <summary>
/// Extension methods to register the global exception handler
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
