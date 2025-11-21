using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TafsilkPlatform.Web.Helpers;

/// <summary>
/// Simple helper methods for controllers - makes controller code easier
/// </summary>
public static class ControllerHelper
{
    /// <summary>
    /// Gets the current user's ID from claims - simple and safe
    /// </summary>
    public static Guid? GetUserId(this Controller controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    /// <summary>
    /// Gets the current user's email from claims
    /// </summary>
    public static string? GetUserEmail(this Controller controller)
    {
        return controller.User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the current user's role from claims
    /// </summary>
    public static string? GetUserRole(this Controller controller)
    {
        return controller.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    public static bool HasRole(this Controller controller, string roleName)
    {
        return controller.GetUserRole()?.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Returns a simple error response
    /// </summary>
    public static IActionResult ErrorResponse(this Controller controller, string message, string? redirectAction = null)
    {
        controller.TempData["Error"] = message;
        if (!string.IsNullOrEmpty(redirectAction))
        {
            return controller.RedirectToAction(redirectAction);
        }
        return controller.RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Returns a simple success response
    /// </summary>
    public static IActionResult SuccessResponse(this Controller controller, string message, string? redirectAction = null)
    {
        controller.TempData["Success"] = message;
        if (!string.IsNullOrEmpty(redirectAction))
        {
            return controller.RedirectToAction(redirectAction);
        }
        return controller.RedirectToAction("Index", "Home");
    }
}

