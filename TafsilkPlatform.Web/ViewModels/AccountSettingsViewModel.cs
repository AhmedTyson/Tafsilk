using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels;

public class AccountSettingsViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
  [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
    [Display(Name = "رقم الهاتف")]
  public string? PhoneNumber { get; set; }

    // Profile Picture
    [Display(Name = "الصورة الشخصية")]
    public string? ProfilePictureUrl { get; set; }

    [Display(Name = "تحميل صورة جديدة")]
 public IFormFile? ProfilePictureFile { get; set; }

<<<<<<< Updated upstream
=======
    [Display(Name = "حذف الصورة الشخصية")]
    public bool DeleteProfilePicture { get; set; }

>>>>>>> Stashed changes
 // User Role Information
    [Display(Name = "الدور الحالي")]
    public string CurrentRole { get; set; } = string.Empty;

    public Guid CurrentRoleId { get; set; }

    [Display(Name = "يمكن تغيير الدور")]
    public bool CanChangeRole { get; set; }

    // Role-specific fields
    // For Tailors
    [Display(Name = "اسم المتجر")]
    public string? ShopName { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "سنوات الخبرة")]
    public int? ExperienceYears { get; set; }

    [Display(Name = "نطاق الأسعار")]
    public string? PricingRange { get; set; }

    [Display(Name = "نبذة عني")]
    [DataType(DataType.MultilineText)]
    public string? Bio { get; set; }

    // For Customers
    [Display(Name = "الجنس")]
    public string? Gender { get; set; }

[Display(Name = "تاريخ الميلاد")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    // Notification Preferences
    [Display(Name = "تفعيل إشعارات البريد الإلكتروني")]
    public bool EmailNotifications { get; set; } = true;

    [Display(Name = "تفعيل إشعارات الرسائل النصية")]
    public bool SmsNotifications { get; set; } = true;

    [Display(Name = "تفعيل الإشعارات الترويجية")]
    public bool PromotionalNotifications { get; set; } = true;

    // Role change request
    [Display(Name = "طلب تغيير الدور إلى")]
    public string? RequestedRole { get; set; }
}

public class RoleChangeRequestViewModel
{
    [Required(ErrorMessage = "الدور المطلوب مطلوب")]
    [Display(Name = "الدور المطلوب")]
    public string TargetRole { get; set; } = string.Empty;

    [Required(ErrorMessage = "السبب مطلوب")]
    [StringLength(500, MinimumLength = 20, ErrorMessage = "السبب يجب أن يكون بين 20 و 500 حرف")]
    [Display(Name = "سبب طلب التغيير")]
    [DataType(DataType.MultilineText)]
    public string Reason { get; set; } = string.Empty;

    // Additional fields for Tailor role request
    [Display(Name = "اسم المتجر")]
    public string? ShopName { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "سنوات الخبرة")]
    [Range(0, 100, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 100")]
    public int? ExperienceYears { get; set; }

    [Display(Name = "صورة رخصة العمل")]
    public IFormFile? BusinessLicenseImage { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
    [Display(Name = "تأكيد كلمة المرور الجديدة")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
[DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "تذكرني؟")]
    public bool RememberMe { get; set; }
}

public class CompleteGoogleRegistrationViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
    [Display(Name = "رقم الهاتف")]
 public string? PhoneNumber { get; set; }

    [Display(Name = "نوع الحساب")]
    public string? UserType { get; set; }

    public string? ProfilePictureUrl { get; set; }
}