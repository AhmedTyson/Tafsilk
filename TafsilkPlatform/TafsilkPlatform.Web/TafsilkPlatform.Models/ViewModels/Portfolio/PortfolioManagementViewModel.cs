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

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ContentType { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to zero")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
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

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to zero")]
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

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
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
    public const string MensSuits = "Men's Suits";
    public const string EveningDresses = "Evening Dresses";
    public const string Alterations = "Alterations";
    public const string ChildrenClothing = "Kids Clothing";
    public const string WeddingDresses = "Wedding Dresses";
    public const string Other = "Other";

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
