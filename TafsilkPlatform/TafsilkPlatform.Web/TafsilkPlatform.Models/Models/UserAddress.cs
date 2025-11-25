using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    [Table("UserAddresses")]
    public partial class UserAddress
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Address label is required")]
        [StringLength(100, ErrorMessage = "Label cannot exceed 100 characters")]
        public string Label { get; set; } = null!;

        [Required(ErrorMessage = "Street address is required")]
        [StringLength(255, ErrorMessage = "Street cannot exceed 255 characters")]
        public string Street { get; set; } = null!;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = null!;

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [Display(Name = "Default Address")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Validation method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Latitude.HasValue != Longitude.HasValue)
            {
                yield return new ValidationResult(
                    "Both latitude and longitude must be provided together, or both must be null",
                    new[] { nameof(Latitude), nameof(Longitude) });
            }
        }
    }
}
