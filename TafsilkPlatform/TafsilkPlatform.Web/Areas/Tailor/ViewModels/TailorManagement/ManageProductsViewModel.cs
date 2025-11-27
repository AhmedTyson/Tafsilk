using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.TailorManagement;

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

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name must not exceed 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required")]
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
    [Display(Name = "Product Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    [Display(Name = "Price (EGP)")]
    public decimal Price { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Discounted price must be between 0.01 and 999999.99")]
    [Display(Name = "Discounted Price (Optional)")]
    public decimal? DiscountedPrice { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Sub Category")]
    public string? SubCategory { get; set; }

    [Display(Name = "Size")]
    public string? Size { get; set; }

    [Display(Name = "Color")]
    public string? Color { get; set; }

    [Display(Name = "Material")]
    public string? Material { get; set; }

    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, 10000, ErrorMessage = "Quantity must be between 0 and 10000")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Available for Sale")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Featured Product")]
    public bool IsFeatured { get; set; } = false;

    [Required(ErrorMessage = "Primary image is required")]
    [Display(Name = "Primary Image")]
    public IFormFile? PrimaryImage { get; set; }

    [Display(Name = "Additional Images (Up to 5)")]
    public List<IFormFile>? AdditionalImages { get; set; }

    [Display(Name = "SEO Title")]
    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Description")]
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

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name must not exceed 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required")]
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
    [Display(Name = "Product Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    [Display(Name = "Price (EGP)")]
    public decimal Price { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Discounted price must be between 0.01 and 999999.99")]
    [Display(Name = "Discounted Price (Optional)")]
    public decimal? DiscountedPrice { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Sub Category")]
    public string? SubCategory { get; set; }

    [Display(Name = "Size")]
    public string? Size { get; set; }

    [Display(Name = "Color")]
    public string? Color { get; set; }

    [Display(Name = "Material")]
    public string? Material { get; set; }

    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, 10000, ErrorMessage = "Quantity must be between 0 and 10000")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Available for Sale")]
    public bool IsAvailable { get; set; }

    [Display(Name = "Featured Product")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Update Primary Image")]
    public IFormFile? NewPrimaryImage { get; set; }

    [Display(Name = "Add New Images")]
    public List<IFormFile>? NewAdditionalImages { get; set; }

    public bool HasCurrentPrimaryImage { get; set; }
    public int CurrentAdditionalImagesCount { get; set; }

    [Display(Name = "SEO Title")]
    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Description")]
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
