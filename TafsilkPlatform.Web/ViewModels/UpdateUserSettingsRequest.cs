using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels;

public class UpdateUserSettingsRequest
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    public string? City { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

  [MaxLength(500, ErrorMessage = "النبذة لا يمكن أن تتجاوز 500 حرف")]
    public string? Bio { get; set; }

    // Tailor-specific fields
    public string? ShopName { get; set; }
    public string? Address { get; set; }
    public int? ExperienceYears { get; set; }
    public string? PricingRange { get; set; }

    // Corporate-specific fields
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }

    // Password change
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
  [Compare(nameof(NewPassword), ErrorMessage = "كلمات المرور غير متطابقة")]
    public string? ConfirmNewPassword { get; set; }

    // Notification preferences
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PromotionalNotifications { get; set; }
}
