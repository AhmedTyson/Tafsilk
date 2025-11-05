using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Controllers.Base;

/// <summary>
/// Base controller with common functionality for all controllers
/// Implements common patterns and utilities
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly ILogger _logger;

    protected BaseController(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region User Context Methods

    /// <summary>
    /// Get current authenticated user ID from claims
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">When user ID is not found in claims</exception>
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("User ID not found in claims for user {UserName}", User.Identity?.Name);
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }

    /// <summary>
    /// Try to get current user ID without throwing exception
    /// </summary>
    protected bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out userId);
    }

    /// <summary>
    /// Get current user email from claims
    /// </summary>
    protected string GetUserEmail()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email not found in claims for user ID {UserId}",
           User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            throw new UnauthorizedAccessException("Email not found in claims");
        }

        return email;
    }

    /// <summary>
    /// Get current user role from claims
    /// </summary>
    protected string GetUserRole()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(role))
        {
            _logger.LogWarning("Role not found in claims for user {UserId}", GetUserId());
            throw new UnauthorizedAccessException("Role not found in claims");
        }

        return role;
    }

    /// <summary>
    /// Get current user full name from claims
    /// </summary>
    protected string GetUserFullName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value
              ?? User.FindFirst("FullName")?.Value
      ?? GetUserEmail();
    }

    /// <summary>
    /// Check if current user is in specified role
    /// </summary>
    protected bool IsInRole(string role)
    {
        return User.IsInRole(role);
    }

    #endregion

    #region Response Methods

    /// <summary>
    /// Return success response with message in TempData
    /// </summary>
    protected IActionResult SuccessResponse(string message, string? actionName = null, string? controllerName = null)
    {
        TempData["SuccessMessage"] = message;

        if (!string.IsNullOrEmpty(actionName))
        {
            return RedirectToAction(actionName, controllerName);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Return success response with data (for AJAX/API calls)
    /// </summary>
    protected IActionResult SuccessJsonResponse(string message, object? data = null)
    {
        return Json(new
        {
            success = true,
            message,
            data
        });
    }

    /// <summary>
    /// Return error response with message in TempData
    /// </summary>
    protected IActionResult ErrorResponse(string message, string? returnUrl = null)
    {
        TempData["ErrorMessage"] = message;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Return error response with data (for AJAX/API calls)
    /// </summary>
    protected IActionResult ErrorJsonResponse(string message, object? errors = null)
    {
        return Json(new
        {
            success = false,
            message,
            errors
        });
    }

    /// <summary>
    /// Return warning response with message in TempData
    /// </summary>
    protected IActionResult WarningResponse(string message, string? actionName = null)
    {
        TempData["WarningMessage"] = message;
        return actionName != null ? RedirectToAction(actionName) : RedirectToAction("Index");
    }

    /// <summary>
    /// Return info response with message in TempData
    /// </summary>
    protected IActionResult InfoResponse(string message, string? actionName = null)
    {
        TempData["InfoMessage"] = message;
        return actionName != null ? RedirectToAction(actionName) : RedirectToAction("Index");
    }

    #endregion

    #region Service Result Handling

    /// <summary>
    /// Handle service result and return appropriate IActionResult
    /// </summary>
    protected IActionResult HandleServiceResult<T>(
        ServiceResult<T> result,
        Func<T, IActionResult>? onSuccess = null,
string? successMessage = null)
    {
        if (result.Succeeded && result.Data != null)
        {
            if (!string.IsNullOrEmpty(successMessage))
            {
                TempData["SuccessMessage"] = successMessage;
            }

            return onSuccess?.Invoke(result.Data) ?? Ok(result.Data);
        }

        // Log the error
        _logger.LogWarning("Service operation failed: {Error}. Validation errors: {ValidationErrors}",
   result.ErrorMessage,
            result.ValidationErrors != null ? string.Join(", ", result.ValidationErrors) : "None");

        // Handle validation errors
        if (result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            foreach (var error in result.ValidationErrors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        return ErrorResponse(result.ErrorMessage ?? "حدث خطأ غير متوقع");
    }

    /// <summary>
    /// Handle async service result with JSON response
    /// </summary>
    protected IActionResult HandleServiceResultJson<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return SuccessJsonResponse("تمت العملية بنجاح", result.Data);
        }

        return ErrorJsonResponse(
     result.ErrorMessage ?? "حدث خطأ غير متوقع",
               result.ValidationErrors);
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    /// Validate model and return error response if invalid
    /// </summary>
    protected IActionResult? ValidateModelOrReturnError()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
     .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage)
              .ToList();

            _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));

            TempData["ErrorMessage"] = "يرجى تصحيح الأخطاء في النموذج";
            return View();
        }

        return null;
    }

    /// <summary>
    /// Add model errors from list of strings
    /// </summary>
    protected void AddModelErrors(List<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    #endregion

    #region Logging Helpers

    /// <summary>
    /// Log user action for audit trail
    /// </summary>
    protected void LogUserAction(string action, string? details = null)
    {
        try
        {
            var userId = GetUserId();
            var userEmail = GetUserEmail();

            _logger.LogInformation(
           "User action: {Action} by {UserEmail} (ID: {UserId}). Details: {Details}",
          action, userEmail, userId, details ?? "None");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log user action: {Action}", action);
        }
    }

    /// <summary>
    /// Log error with user context
    /// </summary>
    protected void LogError(Exception ex, string message)
    {
        try
        {
            var userId = TryGetUserId(out var uid) ? uid.ToString() : "Unknown";
            _logger.LogError(ex, "{Message}. User ID: {UserId}", message, userId);
        }
        catch
        {
            _logger.LogError(ex, message);
        }
    }

    #endregion

    #region Navigation Helpers

    /// <summary>
    /// Redirect to role-specific dashboard
    /// </summary>
    protected IActionResult RedirectToRoleDashboard(string? roleName = null)
    {
        roleName ??= GetUserRole();

        return roleName?.ToLowerInvariant() switch
        {
            "admin" => RedirectToAction("Index", "AdminDashboard"),
            "tailor" => RedirectToAction("Tailor", "Dashboards"),
            "corporate" => RedirectToAction("Corporate", "Dashboards"),
            "customer" => RedirectToAction("Customer", "Dashboards"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    /// <summary>
    /// Get return URL or default to role dashboard
    /// </summary>
    protected string GetReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return Url.Action("Index", "Home") ?? "/";
    }

    #endregion

    #region Authorization Helpers

    /// <summary>
    /// Check if user is authorized for resource
    /// </summary>
    protected bool IsAuthorizedForResource(Guid resourceOwnerId)
    {
        try
        {
            var userId = GetUserId();
            var isAdmin = IsInRole("Admin");

            return userId == resourceOwnerId || isAdmin;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Return forbidden if not authorized
    /// </summary>
    protected IActionResult ForbiddenResponse(string? message = null)
    {
        TempData["ErrorMessage"] = message ?? "ليس لديك صلاحية للوصول إلى هذا المورد";
        return Forbid();
    }

    #endregion
}

/// <summary>
/// Service result wrapper for consistent response handling
/// </summary>
public class ServiceResult<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? ValidationErrors { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }

    public static ServiceResult<T> ValidationFailure(List<string> errors)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            ValidationErrors = errors,
            ErrorMessage = "فشل التحقق من البيانات"
        };
    }

    public static ServiceResult<T> ValidationFailure(string error)
    {
        return ValidationFailure(new List<string> { error });
    }
}
