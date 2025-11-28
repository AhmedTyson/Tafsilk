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

    [Required(ErrorMessage = "Please enter image title")]
    [StringLength(100, ErrorMessage = "Title must not exceed 100 characters")]
    [Display(Name = "Image Title")]
    public string? Title { get; set; }

    [StringLength(50, ErrorMessage = "Category must not exceed 50 characters")]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Estimated Price")]
    [Range(0, 999999, ErrorMessage = "Price must be between 0 and 999999")]
    public decimal? EstimatedPrice { get; set; }

    [Display(Name = "Featured Image")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Before/After Image")]
    public bool IsBeforeAfter { get; set; }

    [Required(ErrorMessage = "Please select an image")]
    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
}

public class EditPortfolioImageViewModel
{
    public Guid Id { get; set; }
    public Guid TailorId { get; set; }

    [Required(ErrorMessage = "Please enter image title")]
    [StringLength(100, ErrorMessage = "Title must not exceed 100 characters")]
    [Display(Name = "Image Title")]
    public string? Title { get; set; }

    [StringLength(50, ErrorMessage = "Category must not exceed 50 characters")]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Estimated Price")]
    [Range(0, 999999, ErrorMessage = "Price must be between 0 and 999999")]
    public decimal? EstimatedPrice { get; set; }

    [Display(Name = "Featured Image")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Before/After Image")]
    public bool IsBeforeAfter { get; set; }

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "New Image")]
    public IFormFile? NewImageFile { get; set; }

    public bool HasCurrentImage { get; set; }
}

#endregion


