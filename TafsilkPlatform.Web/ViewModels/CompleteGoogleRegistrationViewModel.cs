using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels;

public class CompleteGoogleRegistrationViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
[Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "نوع الحساب")]
    public string UserType { get; set; } = "customer";

    public string? ProfilePictureUrl { get; set; }
}
