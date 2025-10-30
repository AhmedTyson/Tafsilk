using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels;

public class UserSettingsViewModel
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

 [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

  [Display(Name = "المدينة")]
    public string? City { get; set; }

  [Display(Name = "تاريخ الميلاد")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "الجنس")]
  public string? Gender { get; set; }

    [MaxLength(500, ErrorMessage = "النبذة لا يمكن أن تتجاوز 500 حرف")]
    [Display(Name = "نبذة عني")]
    public string? Bio { get; set; }

    [Display(Name = "الدور")]
    public string Role { get; set; } = string.Empty;

    // Profile picture
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePicture { get; set; }

    // Tailor-specific fields
    [Display(Name = "اسم المتجر")]
    public string? ShopName { get; set; }

 [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "سنوات الخبرة")]
    [Range(0, 50, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 50")]
    public int? ExperienceYears { get; set; }

    [Display(Name = "نطاق الأسعار")]
    public string? PricingRange { get; set; }

    // Corporate-specific fields
    [Display(Name = "اسم الشركة")]
    public string? CompanyName { get; set; }

  [Display(Name = "الشخص المسؤول")]
    public string? ContactPerson { get; set; }

    // Password change
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string? CurrentPassword { get; set; }

    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "كلمات المرور غير متطابقة")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string? ConfirmNewPassword { get; set; }

    // Notification preferences
    [Display(Name = "إشعارات البريد الإلكتروني")]
    public bool EmailNotifications { get; set; }

    [Display(Name = "إشعارات الرسائل النصية")]
  public bool SmsNotifications { get; set; }

    [Display(Name = "الإشعارات الترويجية")]
    public bool PromotionalNotifications { get; set; }
}
