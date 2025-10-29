using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels
{
 public class UserSettingsViewModel
 {
 public Guid UserId { get; set; }

 [Required(ErrorMessage = "الاسم الكامل مطلوب")]
 [Display(Name = "الاسم الكامل")]
 [StringLength(100, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز100 حرف")]
 public string FullName { get; set; } = string.Empty;

 [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
 [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
 [Display(Name = "البريد الإلكتروني")]
 public string Email { get; set; } = string.Empty;

 [Display(Name = "رقم الهاتف")]
 [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
 public string? PhoneNumber { get; set; }

 [Display(Name = "المدينة")]
 [StringLength(50, ErrorMessage = "اسم المدينة يجب ألا يتجاوز50 حرف")]
 public string? City { get; set; }

 [Display(Name = "تاريخ الميلاد")]
 [DataType(DataType.Date)]
 public DateTime? DateOfBirth { get; set; }

 [Display(Name = "نبذة عني")]
 [StringLength(500, ErrorMessage = "النبذة يجب ألا تتجاوز500 حرف")]
 public string? Bio { get; set; }

 [Display(Name = "صورة الملف الشخصي")]
 public IFormFile? ProfilePicture { get; set; }

 public string? ProfilePictureUrl { get; set; }

 // Password change fields
 [Display(Name = "كلمة المرور الحالية")]
 [DataType(DataType.Password)]
 public string? CurrentPassword { get; set; }

 [Display(Name = "كلمة المرور الجديدة")]
 [DataType(DataType.Password)]
 [StringLength(100, MinimumLength =6, ErrorMessage = "كلمة المرور يجب أن تكون بين6 و100 حرف")]
 public string? NewPassword { get; set; }

 [Display(Name = "تأكيد كلمة المرور الجديدة")]
 [DataType(DataType.Password)]
 [Compare("NewPassword", ErrorMessage = "كلمات المرور غير متطابقة")]
 public string? ConfirmNewPassword { get; set; }

 // Notification preferences
 [Display(Name = "تفعيل الإشعارات البريدية")]
 public bool EmailNotifications { get; set; } = true;

 [Display(Name = "تفعيل إشعارات SMS")]
 public bool SmsNotifications { get; set; } = true;

 [Display(Name = "تفعيل الإشعارات الترويجية")]
 public bool PromotionalNotifications { get; set; } = true;

 // Role-specific properties
 public string Role { get; set; } = string.Empty;
 
 // For Tailors
 [Display(Name = "اسم المحل")]
 public string? ShopName { get; set; }

 [Display(Name = "العنوان")]
 public string? Address { get; set; }

<<<<<<< Updated upstream
=======
 [Display(Name = "سنوات الخبرة")]
 public int? ExperienceYears { get; set; }

 [Display(Name = "نطاق الأسعار")]
 public string? PricingRange { get; set; }

 [Display(Name = "الجنس")]
 public string? Gender { get; set; }

>>>>>>> Stashed changes
 // For Corporate
 [Display(Name = "اسم الشركة")]
 public string? CompanyName { get; set; }

 [Display(Name = "الشخص المسؤول")]
 public string? ContactPerson { get; set; }
 }

 public class UpdateUserSettingsRequest
 {
 public string FullName { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string? PhoneNumber { get; set; }
 public string? City { get; set; }
 public DateTime? DateOfBirth { get; set; }
 public string? Bio { get; set; }
 public string? CurrentPassword { get; set; }
 public string? NewPassword { get; set; }
 public bool EmailNotifications { get; set; }
 public bool SmsNotifications { get; set; }
 public bool PromotionalNotifications { get; set; }
 public string? ShopName { get; set; }
 public string? Address { get; set; }
 public string? CompanyName { get; set; }
 public string? ContactPerson { get; set; }
<<<<<<< Updated upstream
=======
 public int? ExperienceYears { get; set; }
 public string? PricingRange { get; set; }
 public string? Gender { get; set; }
>>>>>>> Stashed changes
 }
}
