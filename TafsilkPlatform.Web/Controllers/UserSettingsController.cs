using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Controllers;

[Authorize]
public class UserSettingsController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(IUserService userService, ILogger<UserSettingsController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

<<<<<<< Updated upstream
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Account");

        var settings = await _userService.GetUserSettingsAsync(userId);
        if (settings == null)
        {
            TempData["Error"] = "تعذر تحميل إعدادات المستخدم";
            return RedirectToAction("Index", "Home");
        }

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Account");

        if (model.ProfilePicture != null)
        {
            var success = await _userService.UpdateProfilePictureAsync(userId, model.ProfilePicture);
            if (!success)
            {
                ModelState.AddModelError("ProfilePicture", "تعذر رفع صورة الملف الشخصي. يرجى التأكد من أن الملف صورة بحجم أقل من5MB");
                return View(model);
            }
        }

        var request = new UpdateUserSettingsRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            City = model.City,
            DateOfBirth = model.DateOfBirth,
            Bio = model.Bio,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            EmailNotifications = model.EmailNotifications,
            SmsNotifications = model.SmsNotifications,
            PromotionalNotifications = model.PromotionalNotifications,
            ShopName = model.ShopName,
            Address = model.Address,
            CompanyName = model.CompanyName,
            ContactPerson = model.ContactPerson
        };

        var (succeeded, error) = await _userService.UpdateUserSettingsAsync(userId, request);
        if (succeeded)
        {
            TempData["Success"] = "تم حفظ التعديلات بنجاح";
            return RedirectToAction("Edit");
        }
        ModelState.AddModelError(string.Empty, error ?? "حدث خطأ أثناء حفظ التعديلات");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProfilePicture()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Account");

        var user = await _userService.GetUserSettingsAsync(userId);
        if (user == null)
            return NotFound();

        TempData["Success"] = "تم حذف صورة الملف الشخصي بنجاح";
        return RedirectToAction("Edit");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)) ? userId : Guid.Empty;
=======
    /// <summary>
    /// Displays the user settings page - This action should ALWAYS be accessible to authenticated users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      return await Edit();
    }

    /// <summary>
    /// Main settings view - Protected to ensure it's always available
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
     var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
         _logger.LogWarning("Unauthorized access attempt to settings page");
       return RedirectToAction("Login", "Account");
 }

        try
        {
       var settings = await _userService.GetUserSettingsAsync(userId);
  if (settings == null)
  {
                _logger.LogError("Unable to load user settings for user: {UserId}", userId);
     TempData["Error"] = "تعذر تحميل إعدادات المستخدم. يرجى المحاولة مرة أخرى.";
                
         // Don't redirect away - show error but keep user on settings page
        // Create a minimal view model to prevent crashes
     settings = new UserSettingsViewModel
         {
              UserId = userId,
 FullName = User.Identity?.Name ?? "مستخدم",
        Email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
     Role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Customer"
              };
       }

  return View(settings);
        }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings for user: {UserId}", userId);
            TempData["Error"] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.";
          return View(new UserSettingsViewModel { UserId = userId });
        }
    }

    /// <summary>
    /// Updates user settings with comprehensive validation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(UserSettingsViewModel model)
    {
        if (!ModelState.IsValid)
 {
            TempData["Error"] = "يرجى التحقق من صحة البيانات المدخلة";
 return View(model);
   }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Unauthorized settings update attempt");
          return RedirectToAction("Login", "Account");
        }

    // Security check: Ensure user can only update their own settings
        if (model.UserId != userId)
        {
        _logger.LogWarning("User {UserId} attempted to update settings for user {TargetUserId}", userId, model.UserId);
TempData["Error"] = "غير مصرح لك بتعديل هذه الإعدادات";
            return RedirectToAction("Edit");
     }

        try
        {
            // Handle profile picture upload separately to avoid data loss
       if (model.ProfilePicture != null)
    {
   var uploadSuccess = await _userService.UpdateProfilePictureAsync(userId, model.ProfilePicture);
    if (!uploadSuccess)
   {
       ModelState.AddModelError("ProfilePicture", "تعذر رفع صورة الملف الشخصي. يرجى التأكد من أن الملف صورة بحجم أقل من 5MB");
             TempData["Warning"] = "لم يتم تحديث صورة الملف الشخصي";
  // Continue with other updates
       }
      else
          {
           TempData["ProfilePictureSuccess"] = "تم تحديث صورة الملف الشخصي بنجاح";
     }
        }

 // Update settings
      var request = new UpdateUserSettingsRequest
          {
    FullName = model.FullName,
  Email = model.Email,
        PhoneNumber = model.PhoneNumber,
           City = model.City,
          DateOfBirth = model.DateOfBirth,
    Bio = model.Bio,
     CurrentPassword = model.CurrentPassword,
     NewPassword = model.NewPassword,
   EmailNotifications = model.EmailNotifications,
       SmsNotifications = model.SmsNotifications,
   PromotionalNotifications = model.PromotionalNotifications,
              ShopName = model.ShopName,
        Address = model.Address,
        CompanyName = model.CompanyName,
  ContactPerson = model.ContactPerson
            };

 var (succeeded, error) = await _userService.UpdateUserSettingsAsync(userId, request);
            
 if (succeeded)
       {
         _logger.LogInformation("Settings updated successfully for user: {UserId}", userId);
     TempData["Success"] = "تم حفظ التعديلات بنجاح";
      return RedirectToAction("Edit");
            }
     
     _logger.LogWarning("Failed to update settings for user {UserId}: {Error}", userId, error);
      ModelState.AddModelError(string.Empty, error ?? "حدث خطأ أثناء حفظ التعديلات");
       return View(model);
        }
      catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while updating settings for user: {UserId}", userId);
            TempData["Error"] = "حدث خطأ غير متوقع أثناء حفظ التعديلات. يرجى المحاولة مرة أخرى.";
       return View(model);
        }
    }

    /// <summary>
    /// Removes profile picture with confirmation - Protected against accidental deletion
    /// </summary>
    [HttpPost]
  [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProfilePicture()
    {
var userId = GetCurrentUserId();
      if (userId == Guid.Empty)
 {
    _logger.LogWarning("Unauthorized profile picture removal attempt");
     return RedirectToAction("Login", "Account");
    }

        try
   {
        var user = await _userService.GetUserSettingsAsync(userId);
if (user == null)
         {
    _logger.LogError("User not found for profile picture removal: {UserId}", userId);
        TempData["Error"] = "تعذر العثور على المستخدم";
    return RedirectToAction("Edit");
   }

         // Check if user actually has a profile picture
   if (string.IsNullOrEmpty(user.ProfilePictureUrl))
{
  TempData["Warning"] = "لا توجد صورة ملف شخصي لحذفها";
    return RedirectToAction("Edit");
       }

       // Remove the profile picture
            var success = await _userService.RemoveProfilePictureAsync(userId);
            
   if (success)
            {
   _logger.LogInformation("Profile picture removed for user: {UserId}", userId);
         TempData["Success"] = "تم حذف صورة الملف الشخصي بنجاح";
            }
            else
            {
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
 /// Helper method to get current user ID from claims
    /// </summary>
    private Guid GetCurrentUserId()
 {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
return userId;
 }

  _logger.LogWarning("Unable to extract user ID from claims");
     return Guid.Empty;
>>>>>>> Stashed changes
    }
}
