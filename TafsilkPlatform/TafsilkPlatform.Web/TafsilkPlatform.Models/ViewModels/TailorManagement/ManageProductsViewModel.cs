using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.TailorManagement;

/// <summary>
/// ViewModel for managing tailor's products
/// </summary>
public class ManageProductsViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<ProductItemDto> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
}

/// <summary>
/// Product item DTO for list display
/// </summary>
public class ProductItemDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public double AverageRating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool HasImage { get; set; }
}

/// <summary>
/// ViewModel for adding a new product
/// </summary>
public class AddProductViewModel
{
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(200, ErrorMessage = "اسم المنتج يجب أن لا يتجاوز 200 حرف")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف المنتج مطلوب")]
    [StringLength(2000, ErrorMessage = "الوصف يجب أن لا يتجاوز 2000 حرف")]
    [Display(Name = "وصف المنتج")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0.01, 999999.99, ErrorMessage = "السعر يجب أن يكون بين 0.01 و 999999.99")]
    [Display(Name = "السعر (ريال)")]
    public decimal Price { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "سعر الخصم يجب أن يكون بين 0.01 و 999999.99")]
    [Display(Name = "سعر الخصم (اختياري)")]
    public decimal? DiscountedPrice { get; set; }

    [Required(ErrorMessage = "التصنيف مطلوب")]
    [Display(Name = "التصنيف")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "التصنيف الفرعي")]
    public string? SubCategory { get; set; }

    [Display(Name = "المقاس")]
    public string? Size { get; set; }

    [Display(Name = "اللون")]
    public string? Color { get; set; }

    [Display(Name = "الخامة")]
    public string? Material { get; set; }

    [Display(Name = "العلامة التجارية")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "الكمية المتوفرة مطلوبة")]
    [Range(0, 10000, ErrorMessage = "الكمية يجب أن تكون بين 0 و 10000")]
    [Display(Name = "الكمية المتوفرة")]
    public int StockQuantity { get; set; }

    [Display(Name = "متاح للبيع")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "منتج مميز")]
    public bool IsFeatured { get; set; } = false;

    [Required(ErrorMessage = "الصورة الأساسية مطلوبة")]
    [Display(Name = "الصورة الأساسية")]
    public IFormFile? PrimaryImage { get; set; }

    [Display(Name = "صور إضافية (حتى 5 صور)")]
    public List<IFormFile>? AdditionalImages { get; set; }

    [Display(Name = "عنوان SEO")]
    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [Display(Name = "وصف SEO")]
    [StringLength(500)]
    public string? MetaDescription { get; set; }
}

/// <summary>
/// ViewModel for editing an existing product
/// </summary>
public class EditProductViewModel
{
    public Guid ProductId { get; set; }
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(200, ErrorMessage = "اسم المنتج يجب أن لا يتجاوز 200 حرف")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف المنتج مطلوب")]
    [StringLength(2000, ErrorMessage = "الوصف يجب أن لا يتجاوز 2000 حرف")]
    [Display(Name = "وصف المنتج")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0.01, 999999.99, ErrorMessage = "السعر يجب أن يكون بين 0.01 و 999999.99")]
    [Display(Name = "السعر (ريال)")]
    public decimal Price { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "سعر الخصم يجب أن يكون بين 0.01 و 999999.99")]
    [Display(Name = "سعر الخصم (اختياري)")]
    public decimal? DiscountedPrice { get; set; }

    [Required(ErrorMessage = "التصنيف مطلوب")]
    [Display(Name = "التصنيف")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "التصنيف الفرعي")]
    public string? SubCategory { get; set; }

    [Display(Name = "المقاس")]
    public string? Size { get; set; }

    [Display(Name = "اللون")]
    public string? Color { get; set; }

    [Display(Name = "الخامة")]
    public string? Material { get; set; }

    [Display(Name = "العلامة التجارية")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "الكمية المتوفرة مطلوبة")]
    [Range(0, 10000, ErrorMessage = "الكمية يجب أن تكون بين 0 و 10000")]
    [Display(Name = "الكمية المتوفرة")]
    public int StockQuantity { get; set; }

    [Display(Name = "متاح للبيع")]
    public bool IsAvailable { get; set; }

    [Display(Name = "منتج مميز")]
    public bool IsFeatured { get; set; }

    [Display(Name = "تحديث الصورة الأساسية")]
    public IFormFile? NewPrimaryImage { get; set; }

    [Display(Name = "إضافة صور جديدة")]
    public List<IFormFile>? NewAdditionalImages { get; set; }

    public bool HasCurrentPrimaryImage { get; set; }
    public int CurrentAdditionalImagesCount { get; set; }

    [Display(Name = "عنوان SEO")]
    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [Display(Name = "وصف SEO")]
    [StringLength(500)]
    public string? MetaDescription { get; set; }
}

/// <summary>
/// ViewModel for quick stock update
/// </summary>
public class QuickStockUpdateViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }

    [Required]
    [Range(0, 10000)]
    public int NewStock { get; set; }
}
