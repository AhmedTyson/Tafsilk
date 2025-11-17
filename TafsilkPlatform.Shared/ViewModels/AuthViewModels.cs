using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Shared.ViewModels
{
    /// <summary>
    /// Shared login view model
    /// </summary>
    public class SharedLoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Shared registration view model
 /// </summary>
    public class SharedRegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
[DataType(DataType.Password)]
 public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
 public string ConfirmPassword { get; set; } = string.Empty;

     [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدور مطلوب")]
        public string Role { get; set; } = "Customer";
    }

    /// <summary>
    /// Shared profile update request
    /// </summary>
    public class UpdateProfileRequest
{
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
     [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "المدينة مطلوبة")]
     public string City { get; set; } = string.Empty;
    }
}
