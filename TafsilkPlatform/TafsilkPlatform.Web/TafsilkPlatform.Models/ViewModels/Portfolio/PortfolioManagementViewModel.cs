using TafsilkPlatform.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.Portfolio;

/// <summary>
/// ViewModel for managing tailor portfolio
/// </summary>
public class PortfolioManagementViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public List<PortfolioImageDto> Images { get; set; } = new();
    public PortfolioStats Stats { get; set; } = new();
}

/// <summary>
/// DTO for portfolio image display
/// </summary>
public class PortfolioImageDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(100, ErrorMessage = "العنوان لا يمكن أن يتجاوز 100 حرف")]
    public string Title { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ContentType { get; set; }

    [Required(ErrorMessage = "الفئة مطلوبة")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>
/// Portfolio statistics
/// </summary>
public class PortfolioStats
{
    public int TotalImages { get; set; }
    public int MonthlyViews { get; set; }
    public int WeeklyAppointments { get; set; }
    public int CompletedProjects { get; set; }
}

/// <summary>
/// ViewModel for uploading portfolio image
/// </summary>
public class UploadPortfolioImageViewModel
{
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "الفئة مطلوبة")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsFeatured { get; set; }
}

/// <summary>
/// ViewModel for updating portfolio image details
/// </summary>
public class UpdatePortfolioImageViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "الفئة مطلوبة")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsFeatured { get; set; }
}

/// <summary>
/// ViewModel for reordering portfolio images
/// </summary>
public class ReorderPortfolioViewModel
{
    public List<Guid> ImageIds { get; set; } = new();
}

/// <summary>
/// Categories for portfolio images
/// </summary>
public static class PortfolioCategories
{
    public const string MensSuits = "بدل رجالية";
    public const string EveningDresses = "فساتين سهرة";
    public const string Alterations = "تعديلات";
    public const string ChildrenClothing = "ملابس أطفال";
    public const string WeddingDresses = "فساتين زفاف";
    public const string Other = "أخرى";

    public static List<string> GetAll()
    {
        return new List<string>
        {
   MensSuits,
            EveningDresses,
            Alterations,
    ChildrenClothing,
   WeddingDresses,
    Other
        };
    }
}
