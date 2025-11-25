using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    [Table("CustomerProfiles")]
    public partial class CustomerProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        public string? Gender { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [Obsolete("Use ProfilePictureData instead. This field is kept for backward compatibility.")]
        public string? ProfilePictureUrl { get; set; }

        // New properties for storing image in database
        [MaxLength]
        public byte[]? ProfilePictureData { get; set; }

        [StringLength(100)]
        public string? ProfilePictureContentType { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(CustomerProfile), nameof(ValidateDateOfBirth))]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public List<Order> orders { get; set; } = new();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        
        // ✅ NEW: Shopping cart
        public virtual ShoppingCart? ShoppingCart { get; set; }
        
        // ✅ NEW: Loyalty and rewards
        public virtual CustomerLoyalty? Loyalty { get; set; }
     
        // ✅ NEW: Saved measurements for faster rebooking
        public virtual ICollection<CustomerMeasurement> SavedMeasurements { get; set; } = new List<CustomerMeasurement>();
 
        // ✅ NEW: Complaints and support
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        
        // ✅ NEW: Product reviews
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        // Custom validation method
        public static ValidationResult? ValidateDateOfBirth(DateOnly? dateOfBirth, ValidationContext context)
        {
            if (dateOfBirth.HasValue)
            {
                var minDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-120));
                var maxDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-13));

                if (dateOfBirth < minDate)
                    return new ValidationResult("Date of birth is too far in the past");

                if (dateOfBirth > maxDate)
                    return new ValidationResult("User must be at least 13 years old");
            }
            return ValidationResult.Success;
        }
    }
}
