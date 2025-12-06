using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class RoleChangeRequestViewModel
{
    [Required(ErrorMessage = "Target role is required")]
    [Display(Name = "Requested Role")]
    public string TargetRole { get; set; } = "Tailor";

    [Required(ErrorMessage = "Shop name is required")]
    [StringLength(100, ErrorMessage = "Shop name must not exceed 100 characters")]
    [Display(Name = "Shop Name")]
    public string ShopName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address must not exceed 200 characters")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Experience years is required")]
    [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
    [Display(Name = "Years of Experience")]
    public int ExperienceYears { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500, MinimumLength = 20, ErrorMessage = "Reason must be between 20 and 500 characters")]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
}

