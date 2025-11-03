using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TafsilkPlatform.Web.Interfaces;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Middleware to check user active status and verification/approval status
/// CRITICAL FOR TAILORS: Ensures incomplete registrations are redirected to verification
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

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IUnitOfWork unitOfWork)
  {
        // Skip for unauthenticated users
 if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Skip for certain paths (login, logout, static files, etc.)
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (ShouldSkipMiddleware(path))
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

            // CRITICAL: Check tailor verification status
        var roleName = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleName?.Equals("Tailor", StringComparison.OrdinalIgnoreCase) == true)
    {
      await HandleTailorVerificationCheck(context, userId, unitOfWork);
            }
        }
        catch (Exception ex)
     {
     _logger.LogError(ex, "[UserStatusMiddleware] Error checking user status");
      }

        await _next(context);
    }

    /// <summary>
    /// CRITICAL: Ensures tailors have completed verification before accessing tailor features
    /// Redirects incomplete registrations to CompleteTailorProfile page
    /// </summary>
    private async Task HandleTailorVerificationCheck(HttpContext context, Guid userId, IUnitOfWork unitOfWork)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Allow access to these pages for unverified tailors
        if (path.Contains("/account/completetailorprofile") ||
     path.Contains("/account/logout") ||
         path.Contains("/home"))
        {
            return;
        }

 // Check if tailor has completed verification (profile exists)
      var tailorProfile = await unitOfWork.Tailors.GetByUserIdAsync(userId);

        // Enforces mandatory verification
 if (tailorProfile == null)
 {
 // MANDATORY REDIRECT - Cannot be bypassed
            _logger.LogWarning("[UserStatusMiddleware] Tailor {UserId} attempted to access {Path} without completing verification. Redirecting to evidence page.", 
      userId, path);
   
   context.Response.Redirect("/Account/CompleteTailorProfile?incomplete=true");
            return;
        }
 else if (!tailorProfile.IsVerified)
        {
  // Tailor has submitted evidence but not yet approved by admin
            // Allow limited access but show warning
            if (path.Contains("/dashboards/tailor") || 
     path.Contains("/tailormanagement") ||
         path.Contains("/profiles/tailorprofile"))
            {
           // Add a flag to show "pending approval" notice
           context.Items["PendingApproval"] = true;
       }
 }
    }

    /// <summary>
    /// Determines if middleware should skip processing for this path
    /// </summary>
    private bool ShouldSkipMiddleware(string path)
    {
        return path.Contains("/account/login") ||
      path.Contains("/account/logout") ||
    path.Contains("/account/register") ||
               path.Contains("/account/completetailorprofile") ||
     path.Contains("/account/verifyemail") ||
     path.Contains("/account/resendverificationemail") ||
 path.StartsWith("/css") ||
     path.StartsWith("/js") ||
path.StartsWith("/lib") ||
      path.StartsWith("/images") ||
      path.StartsWith("/uploads") ||
      path.StartsWith("/favicon") ||
               path.StartsWith("/health") ||
   path.StartsWith("/swagger") ||
          path.StartsWith("/_framework") ||
               path.StartsWith("/_vs");
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
