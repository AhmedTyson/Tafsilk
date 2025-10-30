using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels;

public class AccountSettingsViewModel
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

    [Display(Name = "الدور الحالي")]
    public string CurrentRole { get; set; } = string.Empty;

    public Guid CurrentRoleId { get; set; }

    public bool CanChangeRole { get; set; }

    // Profile picture
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePictureFile { get; set; }

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

    // Notification preferences
    [Display(Name = "إشعارات البريد الإلكتروني")]
    public bool EmailNotifications { get; set; }

    [Display(Name = "إشعارات الرسائل النصية")]
    public bool SmsNotifications { get; set; }

    [Display(Name = "الإشعارات الترويجية")]
    public bool PromotionalNotifications { get; set; }
}
