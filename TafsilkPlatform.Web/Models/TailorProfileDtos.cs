using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Models
{
    public class TailorProfileCreateDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Shop name is required")]
        [StringLength(255, ErrorMessage = "Shop name cannot exceed 255 characters")]
        public string ShopName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        [StringLength(100, ErrorMessage = "Pricing range cannot exceed 100 characters")]
        public string? PricingRange { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string? Bio { get; set; }
    }

    public class TailorProfileUpdateDto
    {
        [StringLength(255, ErrorMessage = "Shop name cannot exceed 255 characters")]
        public string? ShopName { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
        public int? ExperienceYears { get; set; }

        [StringLength(100, ErrorMessage = "Pricing range cannot exceed 100 characters")]
        public string? PricingRange { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string? Bio { get; set; }

        [Display(Name = "Verified Status")]
        public bool? IsVerified { get; set; }
    }

    public class TailorProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? ExperienceYears { get; set; }
        public string? PricingRange { get; set; }
        public string? Bio { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserEmail { get; set; }
    }
}
