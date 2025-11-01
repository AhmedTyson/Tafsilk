using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels;

public class RegisterRequest
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
  [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
  public string? PhoneNumber { get; set; }

    public RegistrationRole Role { get; set; } = RegistrationRole.Customer;

    // Tailor-specific fields
    public string? ShopName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

    // Corporate-specific fields
    public string? CompanyName { get; set; }
 public string? ContactPerson { get; set; }
}
