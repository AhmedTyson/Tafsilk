using System.Net;
using System.Text.Json;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Global exception handler middleware - catches all unhandled exceptions
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);

            // Check if response has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot modify response. Exception: {Exception}", ex.Message);
                // If response has started, we can't modify it, so we need to rethrow
                // But we'll log it first
                throw;
            }

            try
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception handlerEx)
            {
                _logger.LogCritical(handlerEx, "CRITICAL: Error in exception handler itself! Original exception: {OriginalException}", ex.Message);
                // If we can't handle the exception, we must rethrow it
                throw;
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        try
        {
            // Clear any existing response
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Check if this is an API request or MVC request
                var isApiRequest = context.Request.Path.StartsWithSegments("/api") ||
                                   context.Request.Headers["Accept"].ToString().Contains("application/json");

                if (isApiRequest)
                {
                    context.Response.ContentType = "application/json";

                    object response;

                    if (_environment.IsDevelopment())
                    {
                        response = new
                        {
                            error = new
                            {
                                message = exception.Message,
                                stackTrace = exception.StackTrace,
                                innerException = exception.InnerException?.Message,
                                requestId = context.TraceIdentifier
                            }
                        };
                    }
                    else
                    {
                        response = new
                        {
                            error = new
                            {
                                message = "An error occurred while processing your request. Please try again later.",
                                requestId = context.TraceIdentifier
                            }
                        };
                    }

                    var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await context.Response.WriteAsync(jsonResponse);
                }
                else
                {
                    // For MVC requests, redirect to error page
                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.Redirect($"/Home/Error?statusCode=500&requestId={context.TraceIdentifier}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "CRITICAL: Failed to handle exception. Original exception: {OriginalException}", exception.Message);
            // If we can't handle it, we must rethrow
            throw;
        }
    }
}

