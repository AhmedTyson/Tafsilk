using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    [Table("RefreshTokens")]
    public partial class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Expiration date is required")]
        [Display(Name = "Expires At")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Expiration date must be in the future")]
        public DateTime ExpiresAt { get; set; }

        [Display(Name = "Revoked At")]
        [DataType(DataType.DateTime)]
        public DateTime? RevokedAt { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsActive => RevokedAt == null && !IsExpired;
    }

    // Custom validation attribute for future dates
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime <= DateTime.UtcNow)
                {
                    return new ValidationResult(ErrorMessage ?? "Date must be in the future");
                }
            }
            return ValidationResult.Success;
        }
    }
}
