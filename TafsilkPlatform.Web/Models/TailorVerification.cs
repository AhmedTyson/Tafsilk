using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models;

/// <summary>
/// Stores tailor verification documents and identity information
/// Submitted by tailors during onboarding and reviewed by admins
/// </summary>
[Table("TailorVerifications")]
public class TailorVerification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TailorProfileId { get; set; }

    // Identity Information
    [Required(ErrorMessage = "رقم الهوية مطلوب")]
    [StringLength(50)]
    [Display(Name = "رقم الهوية الوطنية / الإقامة")]
 public string NationalIdNumber { get; set; } = null!;

    [Required(ErrorMessage = "الاسم الكامل كما في الهوية مطلوب")]
    [StringLength(200)]
    [Display(Name = "الاسم الكامل (كما في الهوية)")]
    public string FullLegalName { get; set; } = null!;

    [StringLength(100)]
    [Display(Name = "الجنسية")]
    public string? Nationality { get; set; }

    [Display(Name = "تاريخ الميلاد")]
    [DataType(DataType.Date)]
 public DateTime? DateOfBirth { get; set; }

    // Business Information
    [StringLength(100)]
    [Display(Name = "رقم السجل التجاري (اختياري)")]
    public string? CommercialRegistrationNumber { get; set; }

    [StringLength(100)]
 [Display(Name = "الرخصة المهنية (اختياري)")]
    public string? ProfessionalLicenseNumber { get; set; }

    // Document Storage
    [Required]
    [MaxLength]
  [Display(Name = "صورة الهوية الأمامية")]
    public byte[] IdDocumentFrontData { get; set; } = null!;

    [StringLength(100)]
    public string? IdDocumentFrontContentType { get; set; }

    [MaxLength]
    [Display(Name = "صورة الهوية الخلفية (اختياري)")]
    public byte[]? IdDocumentBackData { get; set; }

    [StringLength(100)]
 public string? IdDocumentBackContentType { get; set; }

    [MaxLength]
    [Display(Name = "السجل التجاري (اختياري)")]
    public byte[]? CommercialRegistrationData { get; set; }

    [StringLength(100)]
    public string? CommercialRegistrationContentType { get; set; }

    [MaxLength]
    [Display(Name = "الرخصة المهنية (اختياري)")]
    public byte[]? ProfessionalLicenseData { get; set; }

    [StringLength(100)]
    public string? ProfessionalLicenseContentType { get; set; }

    // Verification Status
    [Display(Name = "حالة التحقق")]
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    [Display(Name = "تاريخ التقديم")]
    [DataType(DataType.DateTime)]
    public DateTime SubmittedAt { get; set; }

    [Display(Name = "تاريخ المراجعة")]
  [DataType(DataType.DateTime)]
    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByAdminId { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات المراجعة")]
    public string? ReviewNotes { get; set; }

    [StringLength(1000)]
    [Display(Name = "سبب الرفض")]
    public string? RejectionReason { get; set; }

    // Additional Information
    [StringLength(500)]
    [Display(Name = "ملاحظات إضافية من الخياط")]
    public string? AdditionalNotes { get; set; }

    // Navigation Properties
    [ForeignKey("TailorProfileId")]
    public virtual TailorProfile TailorProfile { get; set; } = null!;

    [ForeignKey("ReviewedByAdminId")]
    public virtual User? ReviewedByAdmin { get; set; }
}

/// <summary>
/// Verification status enum
/// </summary>
public enum VerificationStatus
{
    [Display(Name = "قيد الانتظار")]
    Pending = 0,

    [Display(Name = "قيد المراجعة")]
    UnderReview = 1,

    [Display(Name = "تم الموافقة")]
    Approved = 2,

 [Display(Name = "مرفوض")]
    Rejected = 3,

 [Display(Name = "يحتاج معلومات إضافية")]
    NeedsMoreInfo = 4
}
