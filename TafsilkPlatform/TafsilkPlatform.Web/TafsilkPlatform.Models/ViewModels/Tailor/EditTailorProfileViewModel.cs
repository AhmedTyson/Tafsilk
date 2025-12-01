using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.Tailor;

/// <summary>
/// ViewModel for editing tailor profile
/// </summary>
public class EditTailorProfileViewModel
{
    public Guid TailorId { get; set; }
    public Guid UserId { get; set; }

    // Personal Information
    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    // Shop Details
    [Required(ErrorMessage = "Shop Name is required")]
    [StringLength(100, ErrorMessage = "Shop Name cannot exceed 100 characters")]
    [Display(Name = "Shop Name")]
    public string ShopName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Shop Description")]
    public string? ShopDescription { get; set; }

    [StringLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
    [Display(Name = "Specialization")]
    public string? Specialization { get; set; }

    [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
    [Display(Name = "Years of Experience")]
    public int? YearsOfExperience { get; set; }

    // Location
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    [Display(Name = "District")]
    public string? District { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Detailed Address")]
    public string? Address { get; set; }

    [Display(Name = "Longitude")]
    public decimal? Longitude { get; set; }

    [Display(Name = "Latitude")]
    public decimal? Latitude { get; set; }

    // Bio
    [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    // Profile Picture
    [Display(Name = "Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }

    public string? CurrentProfilePictureUrl { get; set; }
    public byte[]? CurrentProfilePictureData { get; set; }
    public string? CurrentProfilePictureContentType { get; set; }

    // Business Hours
    [Display(Name = "Business Hours")]
    public string? BusinessHours { get; set; }

    // Social Media
    [Url(ErrorMessage = "Invalid Facebook URL")]
    [Display(Name = "Facebook URL")]
    public string? FacebookUrl { get; set; }

    [Url(ErrorMessage = "Invalid Instagram URL")]
    [Display(Name = "Instagram URL")]
    public string? InstagramUrl { get; set; }

    [Phone(ErrorMessage = "Invalid WhatsApp Number")]
    [StringLength(20)]
    [Display(Name = "WhatsApp Number")]
    public string? WhatsAppNumber { get; set; }

    [Url(ErrorMessage = "Invalid Twitter URL")]
    [Display(Name = "Twitter URL")]
    public string? TwitterUrl { get; set; }

    [Url(ErrorMessage = "Invalid Website URL")]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }

    // Statistics (Read-only)
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int PortfolioCount { get; set; }
    public int ServiceCount { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>
/// Available cities for selection
/// </summary>
public static class EgyptCities
{
    public static List<string> GetAll()
    {
        return new List<string>
      {
            "Cairo",
            "Giza",
            "Alexandria",
            "Sharqia",
            "Dakahlia",
            "Qalyubia",
            "Beheira",
            "Monufia",
            "Gharbia",
            "Kafr El Sheikh",
            "Damietta",
            "Port Said",
            "Ismailia",
            "Suez",
            "North Sinai",
            "South Sinai",
            "Red Sea",
            "Fayoum",
            "Beni Suef",
            "Minya",
            "Assiut",
            "Sohag",
            "Qena",
            "Luxor",
            "Aswan",
            "New Valley",
            "Matrouh"
        };
    }
}

/// <summary>
/// Specialization options
/// </summary>
public static class TailorSpecializations
{
    public static List<string> GetAll()
    {
        return new List<string>
        {
            "Men's Suits",
            "Evening Dresses",
            "Wedding Dresses",
            "Kids Clothing",
            "Alterations",
            "Traditional Clothing",
            "Sports Wear",
            "Custom Fashion",
            "All Types"
      };
    }
}
