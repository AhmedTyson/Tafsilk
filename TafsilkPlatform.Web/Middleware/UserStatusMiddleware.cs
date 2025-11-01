using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TafsilkPlatform.Web.Interfaces;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Middleware to check user active status and verification/approval status
/// </summary>
public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserStatusMiddleware> _logger;

    public UserStatusMiddleware(RequestDelegate next, ILogger<UserStatusMiddleware> logger)
    {
 _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
   // Skip for unauthenticated users
  if (context.User.Identity?.IsAuthenticated != true)
        {
         await _next(context);
            return;
 }

  // Skip for certain paths (login, logout, static files, etc.)
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (path.Contains("/account/login") ||
            path.Contains("/account/logout") ||
    path.Contains("/account/register") ||
 path.StartsWith("/css") ||
     path.StartsWith("/js") ||
            path.StartsWith("/lib") ||
       path.StartsWith("/images") ||
            path.StartsWith("/uploads"))
  {
            await _next(context);
         return;
   }

        try
   {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
  {
          await _next(context);
       return;
     }

        // Get user from database to check current status
       var user = await authService.GetUserByIdAsync(userId);
       
       if (user == null)
            {
     _logger.LogWarning("[UserStatusMiddleware] User not found: {UserId}", userId);
 await SignOutUser(context);
    return;
   }

     // Check if user is active
            if (!user.IsActive)
{
_logger.LogWarning("[UserStatusMiddleware] Inactive user attempted access: {UserId}", userId);
                context.Response.Redirect("/Account/Login?error=inactive");
          return;
        }

      // Check if user is deleted
if (user.IsDeleted)
     {
       _logger.LogWarning("[UserStatusMiddleware] Deleted user attempted access: {UserId}", userId);
 await SignOutUser(context);
     return;
    }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UserStatusMiddleware] Error checking user status");
        }

 await _next(context);
    }

    private async Task SignOutUser(HttpContext context)
    {
   await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Redirect("/Account/Login?error=unauthorized");
    }
}

/// <summary>
/// Extension method to add UserStatusMiddleware
/// </summary>
public static class UserStatusMiddlewareExtensions
{
    public static IApplicationBuilder UseUserStatusCheck(this IApplicationBuilder builder)
    {
   return builder.UseMiddleware<UserStatusMiddleware>();
    }
}
