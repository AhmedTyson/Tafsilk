using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class RoleChangeRequestViewModel
{
    [Required(ErrorMessage = "الدور المستهدف مطلوب")]
    [Display(Name = "الدور المطلوب")]
    public string TargetRole { get; set; } = string.Empty;

    [Display(Name = "اسم المتجر")]
    public string? ShopName { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "سنوات الخبرة")]
    [Range(0, 50, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 50")]
    public int ExperienceYears { get; set; }

    [Display(Name = "السبب")]
    [MaxLength(500, ErrorMessage = "السبب لا يمكن أن يتجاوز 500 حرف")]
    public string? Reason { get; set; }

    [Display(Name = "صورة رخصة العمل")]
    public IFormFile? BusinessLicenseImage { get; set; }
}
