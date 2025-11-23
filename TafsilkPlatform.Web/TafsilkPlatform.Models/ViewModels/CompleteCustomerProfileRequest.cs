using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

/// <summary>
/// ViewModel for completing customer profile after registration
/// </summary>
public class CompleteCustomerProfileRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 255 حرف")]
  [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "المدينة مطلوبة")]
    [StringLength(100, ErrorMessage = "اسم المدينة طويل جداً")]
    [Display(Name = "المدينة")]
    public string City { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [StringLength(20, ErrorMessage = "رقم الهاتف طويل جداً")]
  [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [StringLength(20, ErrorMessage = "قيمة الجنس غير صالحة")]
    [Display(Name = "الجنس")]
    public string? Gender { get; set; }

    [StringLength(1000, ErrorMessage = "النبذة طويلة جداً")]
    [Display(Name = "نبذة عنك")]
    public string? Bio { get; set; }

    [Display(Name = "الصورة الشخصية")]
    public IFormFile? ProfilePicture { get; set; }
}
