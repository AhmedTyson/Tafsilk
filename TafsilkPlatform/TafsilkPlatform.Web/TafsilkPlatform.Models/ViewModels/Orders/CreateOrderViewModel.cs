using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.Orders;

public class CreateOrderViewModel
{
    // Tailor Information
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public string? TailorShopName { get; set; }
    public string? TailorCity { get; set; }
    public string? TailorDistrict { get; set; }
    public decimal TailorAverageRating { get; set; }
    public int TailorReviewCount { get; set; }
    public byte[]? TailorProfilePictureData { get; set; }
    public string? TailorProfilePictureContentType { get; set; }

    // Service Selection
    public List<ServiceOptionViewModel> AvailableServices { get; set; } = new();

    [Required(ErrorMessage = "Please select service type")]
    public Guid? SelectedServiceId { get; set; }

    public string? ServiceType { get; set; }

    // Order Details
    [Required(ErrorMessage = "Please enter order description")]
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
    public string? Description { get; set; }

    // Reference Images
    [Display(Name = "Reference Images")]
    public List<IFormFile>? ReferenceImages { get; set; }

    // Measurements
    [Display(Name = "Measurements")]
    public string? Measurements { get; set; }

    [Display(Name = "Additional Notes")]
    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters")]
    public string? AdditionalNotes { get; set; }

    // Appointment
    [Display(Name = "Requested Delivery Date")]
    public DateTimeOffset? DueDate { get; set; }

    [Display(Name = "Preferred Appointment Time")]
    public string? PreferredTime { get; set; }

    // Pricing
    [Display(Name = "Estimated Price")]
    public decimal EstimatedPrice { get; set; }

    public bool IsExpressService { get; set; }

    // Agreement
    [Required(ErrorMessage = "You must agree to the terms and conditions")]
    public bool AgreeToTerms { get; set; }

    // Internal
    public Guid CustomerId { get; set; }
}

public class ServiceOptionViewModel
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceDescription { get; set; }
    public decimal ServicePrice { get; set; }
    public string ServiceIcon { get; set; } = "fa-cut";
}
