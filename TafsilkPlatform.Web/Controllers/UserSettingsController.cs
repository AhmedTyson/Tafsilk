using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for managing user settings and profile information
/// </summary>
[Authorize]
[Route("[controller]")]
public class UserSettingsController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(
        IUserService userService,
 ILogger<UserSettingsController> logger)
    {
   _userService = userService;
    _logger = logger;
    }

 /// <summary>
    /// Display user settings page
    /// GET: /UserSettings
    /// GET: /UserSettings/Index
    /// </summary>
    [HttpGet]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        return await Edit();
}

    /// <summary>
    /// Display edit settings form
    /// GET: /UserSettings/Edit
    /// </summary>
    [HttpGet("[action]")]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
    {
            _logger.LogWarning("Unauthorized access attempt - no valid user ID found");
            TempData["Error"] = "يجب تسجيل الدخول للوصول إلى الإعدادات";
      return RedirectToAction("Login", "Account");
        }

        try
     {
      var settings = await _userService.GetUserSettingsAsync(userId);
     
if (settings == null)
          {
      _logger.LogError("Failed to load settings for user: {UserId}", userId);
           TempData["Error"] = "تعذر تحميل إعدادات المستخدم";
            
    // Create minimal fallback model
    settings = new UserSettingsViewModel
    {
           UserId = userId,
       Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
   FullName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "مستخدم",
   Role = User.FindFirstValue(ClaimTypes.Role) ?? "Customer"
    };
       }

       return View(settings);
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Error loading settings for user: {UserId}", userId);
         TempData["Error"] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.";
        return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Save user settings
    /// POST: /UserSettings/Edit
    /// </summary>
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserSettingsViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
     _logger.LogWarning("Unauthorized settings update attempt");
        return RedirectToAction("Login", "Account");
        }

        // Security check: Ensure user can only update their own settings
        if (model.UserId != userId)
        {
        _logger.LogWarning(
       "Security violation: User {ActualUserId} attempted to update settings for user {TargetUserId}",
        userId, model.UserId);
     TempData["Error"] = "غير مصرح لك بتعديل هذه الإعدادات";
            return RedirectToAction("Edit");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "يرجى التحقق من صحة البيانات المدخلة";
            return View(model);
        }

        try
        {
        bool profilePictureUpdated = false;

            // Handle profile picture upload first (if provided)
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
 {
     _logger.LogInformation("Processing profile picture upload for user: {UserId}", userId);
            
    var uploadSuccess = await _userService.UpdateProfilePictureAsync(userId, model.ProfilePicture);
    
            if (uploadSuccess)
        {
    profilePictureUpdated = true;
      _logger.LogInformation("Profile picture uploaded successfully for user: {UserId}", userId);
                }
              else
        {
         _logger.LogWarning("Profile picture upload failed for user: {UserId}", userId);
                ModelState.AddModelError("ProfilePicture", "تعذر رفع الصورة. تأكد من أن الملف صورة صالحة (JPG, PNG, GIF, WEBP) وأقل من 5 ميجابايت");
        TempData["Warning"] = "لم يتم تحديث صورة الملف الشخصي";
      }
       }

            // Update user settings
          var request = new UpdateUserSettingsRequest
        {
 FullName = model.FullName?.Trim() ?? string.Empty,
    Email = model.Email?.Trim() ?? string.Empty,
     PhoneNumber = model.PhoneNumber?.Trim(),
              City = model.City?.Trim(),
          DateOfBirth = model.DateOfBirth,
         Bio = model.Bio?.Trim(),
            Gender = model.Gender?.Trim(),
  CurrentPassword = model.CurrentPassword,
     NewPassword = model.NewPassword,
        EmailNotifications = model.EmailNotifications,
     SmsNotifications = model.SmsNotifications,
      PromotionalNotifications = model.PromotionalNotifications,
                // Tailor-specific
    ShopName = model.ShopName?.Trim(),
           Address = model.Address?.Trim(),
    ExperienceYears = model.ExperienceYears,
  PricingRange = model.PricingRange?.Trim(),
                // Corporate-specific
       CompanyName = model.CompanyName?.Trim(),
  ContactPerson = model.ContactPerson?.Trim()
            };

    var (succeeded, errorMessage) = await _userService.UpdateUserSettingsAsync(userId, request);

            if (succeeded)
         {
        _logger.LogInformation("Settings updated successfully for user: {UserId}", userId);
         
  // Set success message
    if (profilePictureUpdated)
                {
       TempData["Success"] = "تم حفظ جميع التعديلات بنجاح وتحديث صورة الملف الشخصي! ✅";
}
       else
       {
      TempData["Success"] = "تم حفظ التعديلات بنجاح! ✅";
         }

                // Redirect to refresh the page with updated data
  return RedirectToAction("Edit");
 }
            else
            {
  _logger.LogWarning("Failed to update settings for user {UserId}: {Error}", userId, errorMessage);
 TempData["Error"] = errorMessage ?? "حدث خطأ أثناء حفظ التعديلات";
              ModelState.AddModelError(string.Empty, errorMessage ?? "فشل حفظ التعديلات");
     return View(model);
          }
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Exception while updating settings for user: {UserId}", userId);
            TempData["Error"] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.";
       return View(model);
   }
    }

    /// <summary>
    /// Remove profile picture
    /// POST: /UserSettings/RemoveProfilePicture
    /// </summary>
    [HttpPost("[action]")]
 [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProfilePicture()
    {
     var userId = GetCurrentUserId();
      if (userId == Guid.Empty)
        {
            _logger.LogWarning("Unauthorized profile picture removal attempt");
 return Json(new { success = false, message = "غير مصرح" });
        }

        try
        {
            // Check if user has a profile picture
     var userSettings = await _userService.GetUserSettingsAsync(userId);
            if (userSettings == null || string.IsNullOrEmpty(userSettings.ProfilePictureUrl))
            {
       _logger.LogInformation("No profile picture to remove for user: {UserId}", userId);
      TempData["Warning"] = "لا توجد صورة ملف شخصي لحذفها";
     return RedirectToAction("Edit");
 }

            // Remove the profile picture
var success = await _userService.RemoveProfilePictureAsync(userId);

            if (success)
  {
        _logger.LogInformation("Profile picture removed successfully for user: {UserId}", userId);
          TempData["Success"] = "تم حذف صورة الملف الشخصي بنجاح ✅";
         }
            else
{
          _logger.LogWarning("Failed to remove profile picture for user: {UserId}", userId);
              TempData["Error"] = "تعذر حذف صورة الملف الشخصي";
    }

        return RedirectToAction("Edit");
        }
      catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing profile picture for user: {UserId}", userId);
            TempData["Error"] = "حدث خطأ أثناء حذف الصورة";
    return RedirectToAction("Edit");
}
    }

    /// <summary>
/// Get the current authenticated user's ID
    /// </summary>
    /// <returns>User ID or Guid.Empty if not found</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
         ?? User.FindFirst("sub");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
    return userId;
        }

        _logger.LogWarning("Unable to extract valid user ID from claims");
        return Guid.Empty;
    }
}
