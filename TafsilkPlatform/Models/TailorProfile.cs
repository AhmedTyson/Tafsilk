using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models
{
    [Table("TailorProfiles")]
    public partial class TailorProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Shop name is required")]
        [StringLength(255, ErrorMessage = "Shop name cannot exceed 255 characters")]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; } = null!;

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        [StringLength(100, ErrorMessage = "Pricing range cannot exceed 100 characters")]
        [Display(Name = "Pricing Range")]
        public string? PricingRange { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        [Display(Name = "Verified Status")]
        public bool IsVerified { get; set; } = false;

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
    }
}