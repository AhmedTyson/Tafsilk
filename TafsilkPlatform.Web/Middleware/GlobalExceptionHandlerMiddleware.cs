using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace TafsilkPlatform.Web.Middleware
{
    /// <summary>
    /// Global exception handler middleware for consistent error responses
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
      private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment environment)
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
     _logger.LogError(ex, "An unhandled exception occurred while processing the request");
          await HandleExceptionAsync(context, ex);
    }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
         context.Response.ContentType = "application/json";
  var response = new ErrorResponse();

    switch (exception)
  {
                case UnauthorizedAccessException:
           context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
       response.Message = "Unauthorized access";
             response.StatusCode = (int)HttpStatusCode.Unauthorized;
  break;

 case KeyNotFoundException:
  context.Response.StatusCode = (int)HttpStatusCode.NotFound;
 response.Message = "Resource not found";
  response.StatusCode = (int)HttpStatusCode.NotFound;
            break;

                case ArgumentNullException:
           case ArgumentException:
  context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    response.Message = "Invalid request parameters";
       response.StatusCode = (int)HttpStatusCode.BadRequest;
          break;

   case InvalidOperationException:
  context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
         response.Message = "Invalid operation";
          response.StatusCode = (int)HttpStatusCode.BadRequest;
        break;

       default:
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.Message = "An internal server error occurred";
    response.StatusCode = (int)HttpStatusCode.InternalServerError;
            break;
  }

            // Include detailed error information in development
         if (_environment.IsDevelopment())
            {
         response.DetailedMessage = exception.Message;
                response.StackTrace = exception.StackTrace;
            }

var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

     await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Error response model for consistent API error responses
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
   public string Message { get; set; } = string.Empty;
        public string? DetailedMessage { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Extension methods for registering the global exception handler
    /// </summary>
    public static class GlobalExceptionHandlerExtensions
    {
 public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
{
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
 }
    }
}
