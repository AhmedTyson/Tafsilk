using Microsoft.AspNetCore.Http;
using TafsilkPlatform.Models.Models;
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

    [Required(ErrorMessage = "يرجى اختيار نوع الخدمة")]
    public Guid? SelectedServiceId { get; set; }

    public string? ServiceType { get; set; }

    // Order Details
    [Required(ErrorMessage = "يرجى إدخال وصف الطلب")]
    [StringLength(2000, ErrorMessage = "الوصف يجب أن لا يتجاوز 2000 حرف")]
    public string? Description { get; set; }

    // Reference Images
    [Display(Name = "صور مرجعية")]
    public List<IFormFile>? ReferenceImages { get; set; }

    // Measurements
    [Display(Name = "المقاسات")]
    public string? Measurements { get; set; }

    [Display(Name = "ملاحظات إضافية")]
    [StringLength(1000, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 1000 حرف")]
    public string? AdditionalNotes { get; set; }

    // Appointment
    [Display(Name = "تاريخ التسليم المطلوب")]
    public DateTimeOffset? DueDate { get; set; }

    [Display(Name = "وقت الموعد المفضل")]
    public string? PreferredTime { get; set; }

    // Pricing
    [Display(Name = "السعر التقديري")]
    public decimal EstimatedPrice { get; set; }

    public bool IsExpressService { get; set; }

    // Agreement
    [Required(ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
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
