using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels;

/// <summary>
/// ViewModel for password reset form
/// </summary>
public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "الرمز مطلوب")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "يجب أن تكون كلمة المرور بين 6 و 100 حرف")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; } = string.Empty;

 [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare(nameof(NewPassword), ErrorMessage = "كلمات المرور غير متطابقة")]
    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
