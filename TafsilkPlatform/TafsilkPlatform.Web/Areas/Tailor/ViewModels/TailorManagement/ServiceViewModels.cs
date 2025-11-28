using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TafsilkPlatform.Web.Areas.Tailor.ViewModels.TailorManagement;

public class ManageServicesViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<ServiceItemDto> Services { get; set; } = [];
    public int TotalServices { get; set; }
    public double AveragePrice { get; set; }
}

public class ServiceItemDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double BasePrice { get; set; }
    public string? EstimatedDuration { get; set; }
}

public class AddServiceViewModel
{
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "Service Name is required")]
    [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
    [Display(Name = "Service Name")]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Base Price is required")]
    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000")]
    [Display(Name = "Base Price (EGP)")]
    public double BasePrice { get; set; }

    [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
    [Display(Name = "Estimated Duration")]
    public string? EstimatedDuration { get; set; }
}

public class EditServiceViewModel
{
    public Guid Id { get; set; }
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "Service Name is required")]
    [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
    [Display(Name = "Service Name")]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Base Price is required")]
    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000")]
    [Display(Name = "Base Price (EGP)")]
    public double BasePrice { get; set; }

    [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
    [Display(Name = "Estimated Duration")]
    public string? EstimatedDuration { get; set; }
}

public class ManagePricingViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public string PricingTier { get; set; } = "Standard";
    public List<ServicePriceDto> ServicePrices { get; set; } = [];
    public List<ServicePriceDto> StandardServices { get; set; } = [];
    public List<ServicePriceDto> CustomServices { get; set; } = [];
}

public class ServicePriceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public double CurrentPrice { get; set; }
    
    [Range(0, 100000, ErrorMessage = "Price must be valid")]
    public double NewPrice { get; set; }
}
