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
    }
}
