using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class CompleteTailorProfileRequest
{
    // Basic Information
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    // Identity Verification (OPTIONAL - for future implementation)
    [StringLength(50, ErrorMessage = "رقم الهوية لا يمكن أن يتجاوز 50 حرفاً")]
    [Display(Name = "رقم الهوية الوطنية / الإقامة")]
    public string? NationalIdNumber { get; set; }

    [StringLength(200)]
    [Display(Name = "الاسم الكامل (كما هو مكتوب في الهوية)")]
    public string? FullLegalName { get; set; }

    [Display(Name = "الجنسية")]
    public string? Nationality { get; set; }

    [Display(Name = "تاريخ الميلاد")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    // Business Information
    [Required(ErrorMessage = "اسم الورشة مطلوب")]
    [Display(Name = "اسم الورشة")]
    public string WorkshopName { get; set; } = string.Empty;

    [Display(Name = "رقم السجل التجاري (اختياري)")]
    [StringLength(100)]
    public string? CommercialRegistrationNumber { get; set; }

    [Display(Name = "رقم الرخصة المهنية (اختياري)")]
    [StringLength(100)]
    public string? ProfessionalLicenseNumber { get; set; }

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

    // Document uploads - OPTIONAL (for future implementation)
    [Display(Name = "صورة الهوية الوطنية - الوجه الأمامي")]
    public IFormFile? IdDocumentFront { get; set; }

    [Display(Name = "صورة الهوية الوطنية - الوجه الخلفي (اختياري)")]
    public IFormFile? IdDocumentBack { get; set; }

    /// <summary>
    /// Alias for backward compatibility
    /// </summary>
    [Display(Name = "صورة الهوية الشخصية أو السجل التجاري")]
    public IFormFile? IdDocument
    {
        get => IdDocumentFront;
        set => IdDocumentFront = value;
    }

    [Display(Name = "صورة السجل التجاري (اختياري)")]
    public IFormFile? CommercialRegistration { get; set; }

    [Display(Name = "صورة الرخصة المهنية (اختياري)")]
    public IFormFile? ProfessionalLicense { get; set; }

    [Display(Name = "صور من أعمالك السابقة (اختياري)")]
    public List<IFormFile>? PortfolioImages { get; set; }

    /// <summary>
    /// Alias for PortfolioImages - work samples
    /// </summary>
    [Display(Name = "عينات من الأعمال")]
    public List<IFormFile>? WorkSamples
    {
        get => PortfolioImages;
        set => PortfolioImages = value;
    }

    [Display(Name = "وثائق إضافية (اختياري)")]
    public List<IFormFile>? AdditionalDocuments { get; set; }

    [Display(Name = "ملاحظات إضافية (اختياري)")]
    [StringLength(500)]
    public string? AdditionalNotes { get; set; }

    [Required(ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
    [Display(Name = "أوافق على الشروط والأحكام وسياسة الخصوصية")]
    public bool AgreeToTerms { get; set; }

    // User ID (set from authenticated user or registration)
    public Guid UserId { get; set; }

    // Email and FullName (for display)
    public string? Email { get; set; }
}
