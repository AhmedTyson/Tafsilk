using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class CompleteTailorProfileRequest
{
    // Basic Information
    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    // Identity Verification (OPTIONAL - for future implementation)
    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    [Display(Name = "National ID / Residency Number")]
    public string? NationalIdNumber { get; set; }

    [StringLength(200)]
    [Display(Name = "Full Legal Name (as in ID)")]
    public string? FullLegalName { get; set; }

    [Display(Name = "Nationality")]
    public string? Nationality { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    // Business Information
    [Required(ErrorMessage = "Workshop Name is required")]
    [Display(Name = "Workshop Name")]
    public string WorkshopName { get; set; } = string.Empty;

    [Display(Name = "Commercial Registration Number (Optional)")]
    [StringLength(100)]
    public string? CommercialRegistrationNumber { get; set; }

    [Display(Name = "Professional License Number (Optional)")]
    [StringLength(100)]
    public string? ProfessionalLicenseNumber { get; set; }

    [Required(ErrorMessage = "Workshop Type is required")]
    [Display(Name = "Workshop Type")]
    public string WorkshopType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [Display(Name = "Workshop Address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "City")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Workshop Description is required")]
    [Display(Name = "Workshop Description and Services")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Years of Experience")]
    [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
    public int? ExperienceYears { get; set; }

    // Document uploads - OPTIONAL (for future implementation)
    [Display(Name = "National ID - Front")]
    public IFormFile? IdDocumentFront { get; set; }

    [Display(Name = "National ID - Back (Optional)")]
    public IFormFile? IdDocumentBack { get; set; }

    /// <summary>
    /// Alias for backward compatibility
    /// </summary>
    [Display(Name = "Personal ID or Commercial Registration")]
    public IFormFile? IdDocument
    {
        get => IdDocumentFront;
        set => IdDocumentFront = value;
    }

    [Display(Name = "Commercial Registration (Optional)")]
    public IFormFile? CommercialRegistration { get; set; }

    [Display(Name = "Professional License (Optional)")]
    public IFormFile? ProfessionalLicense { get; set; }

    [Display(Name = "Portfolio Images (Optional)")]
    public List<IFormFile>? PortfolioImages { get; set; }

    /// <summary>
    /// Alias for PortfolioImages - work samples
    /// </summary>
    [Display(Name = "Work Samples")]
    public List<IFormFile>? WorkSamples
    {
        get => PortfolioImages;
        set => PortfolioImages = value;
    }

    [Display(Name = "Additional Documents (Optional)")]
    public List<IFormFile>? AdditionalDocuments { get; set; }

    [Display(Name = "Additional Notes (Optional)")]
    [StringLength(500)]
    public string? AdditionalNotes { get; set; }

    [Required(ErrorMessage = "You must agree to the Terms and Conditions")]
    [Display(Name = "I agree to the Terms and Conditions and Privacy Policy")]
    public bool AgreeToTerms { get; set; }

    // User ID (set from authenticated user or registration)
    public Guid UserId { get; set; }

    // Email and FullName (for display)
    public string? Email { get; set; }
}
