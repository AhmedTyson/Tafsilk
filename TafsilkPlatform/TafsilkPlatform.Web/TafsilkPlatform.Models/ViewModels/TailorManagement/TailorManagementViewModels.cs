using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.TailorManagement;

#region Portfolio ViewModels

public class ManagePortfolioViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<PortfolioItemDto> Images { get; set; } = new();
    public int TotalImages { get; set; }
    public int FeaturedCount { get; set; }
    public int MaxAllowedImages { get; set; }
}

public class PortfolioItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsBeforeAfter { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool HasImageData { get; set; }
}

public class AddPortfolioImageViewModel
{
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال عنوان الصورة")]
    [StringLength(100, ErrorMessage = "العنوان يجب أن لا يتجاوز 100 حرف")]
    [Display(Name = "عنوان الصورة")]
    public string? Title { get; set; }

    [StringLength(50, ErrorMessage = "الفئة يجب أن لا تتجاوز 50 حرف")]
    [Display(Name = "الفئة")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "السعر التقديري")]
    [Range(0, 999999, ErrorMessage = "السعر يجب أن يكون بين 0 و 999999")]
    public decimal? EstimatedPrice { get; set; }

    [Display(Name = "صورة مميزة")]
    public bool IsFeatured { get; set; }

    [Display(Name = "صورة قبل وبعد")]
    public bool IsBeforeAfter { get; set; }

    [Required(ErrorMessage = "يرجى اختيار صورة")]
    [Display(Name = "الصورة")]
    public IFormFile? ImageFile { get; set; }
}

public class EditPortfolioImageViewModel
{
    public Guid Id { get; set; }
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال عنوان الصورة")]
    [StringLength(100, ErrorMessage = "العنوان يجب أن لا يتجاوز 100 حرف")]
    [Display(Name = "عنوان الصورة")]
    public string? Title { get; set; }

    [StringLength(50, ErrorMessage = "الفئة يجب أن لا تتجاوز 50 حرف")]
    [Display(Name = "الفئة")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "السعر التقديري")]
    [Range(0, 999999, ErrorMessage = "السعر يجب أن يكون بين 0 و 999999")]
    public decimal? EstimatedPrice { get; set; }

    [Display(Name = "صورة مميزة")]
    public bool IsFeatured { get; set; }

    [Display(Name = "صورة قبل وبعد")]
    public bool IsBeforeAfter { get; set; }

    [Display(Name = "ترتيب العرض")]
    public int DisplayOrder { get; set; }

    [Display(Name = "صورة جديدة")]
    public IFormFile? NewImageFile { get; set; }

    public bool HasCurrentImage { get; set; }
}

#endregion

#region Service ViewModels

public class ManageServicesViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<ServiceItemDto> Services { get; set; } = new();
    public int TotalServices { get; set; }
    public decimal AveragePrice { get; set; }
}

public class ServiceItemDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int EstimatedDuration { get; set; }
}

public class AddServiceViewModel
{
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال اسم الخدمة")]
    [StringLength(100, ErrorMessage = "اسم الخدمة يجب أن لا يتجاوز 100 حرف")]
    [Display(Name = "اسم الخدمة")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال وصف الخدمة")]
    [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
    [Display(Name = "وصف الخدمة")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال السعر الأساسي")]
    [Range(1, 999999, ErrorMessage = "السعر يجب أن يكون بين 1 و 999999")]
    [Display(Name = "السعر الأساسي (ريال)")]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = "يرجى إدخال المدة التقديرية")]
    [Range(1, 365, ErrorMessage = "المدة يجب أن تكون بين 1 و 365 يوم")]
    [Display(Name = "المدة التقديرية (بالأيام)")]
    public int EstimatedDuration { get; set; }
}

public class EditServiceViewModel
{
    public Guid Id { get; set; }
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال اسم الخدمة")]
    [StringLength(100, ErrorMessage = "اسم الخدمة يجب أن لا يتجاوز 100 حرف")]
    [Display(Name = "اسم الخدمة")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال وصف الخدمة")]
    [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
    [Display(Name = "وصف الخدمة")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال السعر الأساسي")]
    [Range(1, 999999, ErrorMessage = "السعر يجب أن يكون بين 1 و 999999")]
    [Display(Name = "السعر الأساسي (ريال)")]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = "يرجى إدخال المدة التقديرية")]
    [Range(1, 365, ErrorMessage = "المدة يجب أن تكون بين 1 و 365 يوم")]
    [Display(Name = "المدة التقديرية (بالأيام)")]
    public int EstimatedDuration { get; set; }
}

#endregion

#region Pricing ViewModels

public class ManagePricingViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<ServicePriceDto> ServicePrices { get; set; } = new();
}

public class ServicePriceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }

    [Required(ErrorMessage = "يرجى إدخال السعر الجديد")]
    [Range(1, 999999, ErrorMessage = "السعر يجب أن يكون بين 1 و 999999")]
    [Display(Name = "السعر الجديد")]
    public decimal NewPrice { get; set; }
}

#endregion
