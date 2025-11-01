using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels;

public class CompleteTailorProfileRequest
{
    [Required(ErrorMessage = "اسم الورشة مطلوب")]
    [Display(Name = "اسم الورشة")]
    public string WorkshopName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع الورشة مطلوب")]
    [Display(Name = "نوع الورشة")]
    public string WorkshopType { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
 [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [Display(Name = "عنوان الورشة")]
 public string Address { get; set; } = string.Empty;

 [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Required(ErrorMessage = "وصف الورشة مطلوب")]
    [Display(Name = "وصف الورشة والخدمات")]
    public string Description { get; set; } = string.Empty;

  [Display(Name = "سنوات الخبرة")]
    [Range(0, 100, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 100")]
    public int? ExperienceYears { get; set; }

    // Document uploads
    [Required(ErrorMessage = "صورة الهوية مطلوبة")]
    [Display(Name = "صورة الهوية الشخصية")]
    public IFormFile? IdDocument { get; set; }

    [Required(ErrorMessage = "صور معرض الأعمال مطلوبة (3 على الأقل)")]
    [Display(Name = "صور معرض الأعمال")]
    public List<IFormFile>? PortfolioImages { get; set; }

    [Display(Name = "وثائق إضافية")]
    public List<IFormFile>? AdditionalDocuments { get; set; }

    [Required(ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
    [Display(Name = "أوافق على الشروط والأحكام")]
    public bool AgreeToTerms { get; set; }

    // User ID (set from authenticated user)
  public Guid UserId { get; set; }

 // Email and FullName (for display)
    public string? Email { get; set; }
    public string? FullName { get; set; }
}
