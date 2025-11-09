using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    [Table("TailorProfiles")]
    public partial class TailorProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        // Personal Information
        [StringLength(255)]
        public string? FullName { get; set; }

        // Shop Information
        [Required(ErrorMessage = "Shop name is required")]
        [StringLength(255, ErrorMessage = "Shop name cannot exceed 255 characters")]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Shop description cannot exceed 500 characters")]
        [Display(Name = "Shop Description")]
        public string? ShopDescription { get; set; }

        [StringLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        // Location
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; } = null!;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        // Experience & Pricing
        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        /// <summary>
        /// Alias for ExperienceYears for backward compatibility
        /// </summary>
        [NotMapped]
        public int? YearsOfExperience
        {
            get => ExperienceYears;
            set => ExperienceYears = value;
        }

        [StringLength(100, ErrorMessage = "Pricing range cannot exceed 100 characters")]
        [Display(Name = "Pricing Range")]
        public string? PricingRange { get; set; }

        // Bio & Business Info
        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        [StringLength(200)]
        [Display(Name = "Business Hours")]
        public string? BusinessHours { get; set; }

        // Social Media Links
        [StringLength(500)]
        [Url]
        public string? FacebookUrl { get; set; }

        [StringLength(500)]
        [Url]
        public string? InstagramUrl { get; set; }

        [StringLength(500)]
        [Url]
        public string? TwitterUrl { get; set; }

        [StringLength(500)]
        [Url]
        public string? WebsiteUrl { get; set; }

        // Profile Picture
        [Obsolete("Use ProfilePictureData instead. This field is kept for backward compatibility.")]
        public string? ProfilePictureUrl { get; set; }

        [MaxLength]
        public byte[]? ProfilePictureData { get; set; }

        [StringLength(100)]
        public string? ProfilePictureContentType { get; set; }

        // Verification & Statistics
        [Display(Name = "Verified Status")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Verified Date")]
        [DataType(DataType.DateTime)]
        public DateTime? VerifiedAt { get; set; }

        [Column(TypeName = "decimal(3, 2)")]
        [Range(0, 5)]
        public decimal AverageRating { get; set; } = 0;

        // Timestamps
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<TailorService> TailorServices { get; set; } = new List<TailorService>();
        public ICollection<PortfolioImage> PortfolioImages { get; set; } = new List<PortfolioImage>();

        // Computed properties
        [NotMapped]
        public bool HasLocation => Latitude.HasValue && Longitude.HasValue;

        [NotMapped]
        public string ExperienceLevel
        {
            get
            {
                if (!ExperienceYears.HasValue) return "غير محدد";
                return ExperienceYears.Value switch
                {
                    < 2 => "مبتدئ",
                    < 5 => "متوسط",
                    < 10 => "محترف",
                    _ => "خبير"
                };
            }
        }

        // Domain methods
        public void Verify(DateTime verifiedAt)
        {
            IsVerified = true;
            VerifiedAt = verifiedAt;
            UpdatedAt = verifiedAt;
        }

        public void UpdateRating(decimal newAverageRating)
        {
            if (newAverageRating < 0 || newAverageRating > 5)
                throw new ArgumentException("Rating must be between 0 and 5");

            AverageRating = newAverageRating;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsWithinRadius(decimal targetLat, decimal targetLon, double radiusKm)
        {
            if (!HasLocation) return false;

            const double earthRadiusKm = 6371;
            var dLat = DegreesToRadians((double)(targetLat - Latitude!.Value));
            var dLon = DegreesToRadians((double)(targetLon - Longitude!.Value));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians((double)Latitude.Value)) *
                    Math.Cos(DegreesToRadians((double)targetLat)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = earthRadiusKm * c;

            return distance <= radiusKm;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
