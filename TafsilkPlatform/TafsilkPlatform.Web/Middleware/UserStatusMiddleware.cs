using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Web.Interfaces;

namespace TafsilkPlatform.Web.Middleware;

/// <summary>
/// Middleware to check user active status and verification/approval status
/// CRITICAL FOR TAILORS: Ensures incomplete registrations are redirected to verification
/// </summary>
/// <summary>
/// Middleware to check user active status and verification/approval status
/// CRITICAL FOR TAILORS: Ensures incomplete registrations are redirected to verification
/// </summary>
public class UserStatusMiddleware(RequestDelegate next, ILogger<UserStatusMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<UserStatusMiddleware> _logger = logger;

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

            // Check if user is deleted FIRST (before IsActive check)
            if (user.IsDeleted)
            {
                _logger.LogWarning("[UserStatusMiddleware] Deleted user attempted access: {UserId}", userId);
                await SignOutUser(context);
                return;
            }

            // CRITICAL FIX: Check if tailor needs to complete profile BEFORE checking IsActive
            var roleName = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleName?.Equals("Tailor", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check if tailor has completed profile
                var tailorProfile = await unitOfWork.Tailors.GetByUserIdAsync(userId);

                // If no profile exists, allow access to CompleteTailorProfile even if IsActive = false
                if (tailorProfile == null)
                {
                    // Tailor MUST complete profile before accessing anything else
                    if (!path.Contains("/account/completetailorprofile"))
                    {
                        _logger.LogWarning("[UserStatusMiddleware] Tailor {UserId} attempted to access {Path} without completing profile. Redirecting.", userId, path);
                        context.Response.Redirect("/Account/CompleteTailorProfile?incomplete=true");
                        return;
                    }

                    // Allow access to CompleteTailorProfile page (skip IsActive check)
                    await _next(context);
                    return;
                }

                // Profile exists - check verification status
                if (!tailorProfile.IsVerified)
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

            // Check if user is active (AFTER tailor profile check)
            if (!user.IsActive)
            {
                _logger.LogWarning("[UserStatusMiddleware] Inactive user attempted access: {UserId}", userId);
                context.Response.Redirect("/Account/Login?error=inactive");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UserStatusMiddleware] Error checking user status");
        }

        await _next(context);
    }

    /// <summary>
    /// Determines if middleware should skip processing for this path
    /// </summary>
    private static bool ShouldSkipMiddleware(string path)
    {
        return path.Contains("/account/login") ||
      path.Contains("/account/logout") ||
    path.Contains("/account/register") ||
         path.Contains("/account/completetailorprofile") ||
     path.Contains("/account/resendverificationemail") ||
 path.StartsWith("/css") ||
     path.StartsWith("/js") ||
path.StartsWith("/lib") ||
      path.StartsWith("/images") ||
      path.StartsWith("/uploads") ||
      path.StartsWith("/favicon") ||
path.StartsWith("/health") ||
    path.StartsWith("/_framework") ||
       path.StartsWith("/_vs");
    }

    private static async Task SignOutUser(HttpContext context)
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
