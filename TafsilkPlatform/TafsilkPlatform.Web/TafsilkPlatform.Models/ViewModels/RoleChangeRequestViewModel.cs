using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class RoleChangeRequestViewModel
{
    [Required(ErrorMessage = "Target role is required")]
    [Display(Name = "Requested Role")]
    public string TargetRole { get; set; } = string.Empty;

    [Display(Name = "Shop Name")]
    public string? ShopName { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Years of Experience")]
    [Range(0, 50, ErrorMessage = "Experience years must be between 0 and 50")]
    public int ExperienceYears { get; set; }

    [Display(Name = "Reason")]
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

    [Display(Name = "Business License Image")]
    public IFormFile? BusinessLicenseImage { get; set; }
}
