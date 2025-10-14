using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models
{
    [Table("Users")]
    public partial class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password hash is required")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Deleted Status")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
        public virtual TailorProfile? TailorProfile { get; set; }
        public virtual CustomerProfile? CustomerProfile { get; set; }
        public virtual CorporateAccount? CorporateAccount { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

        // Validation method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Email is required",
                    new[] { nameof(Email) });
            }

            if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length < 10)
            {
                yield return new ValidationResult(
                    "Phone number must be at least 10 characters",
                    new[] { nameof(PhoneNumber) });
            }
        }
    }
}