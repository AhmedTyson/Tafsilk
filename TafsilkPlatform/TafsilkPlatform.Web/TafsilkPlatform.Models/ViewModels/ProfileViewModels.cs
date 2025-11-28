using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels
{
    // ==================== CUSTOMER PROFILE ====================

    public class UpdateCustomerProfileRequest
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^01[0-2,5]\d{8}$", ErrorMessage = "Invalid Egyptian Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        [StringLength(500, ErrorMessage = "Preferences cannot exceed 500 characters")]
        public string? Preferences { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }
    }

    // ==================== TAILOR PROFILE ====================

    public class UpdateTailorProfileRequest
    {
        [Required(ErrorMessage = "Shop Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Shop Name must be between 3 and 100 characters")]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Bio is required")]
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^01[0-2,5]\d{8}$", ErrorMessage = "Invalid Egyptian Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        [Range(0, 60, ErrorMessage = "Experience years must be between 0 and 60")]
        public int? ExperienceYears { get; set; }

        public string? SkillLevel { get; set; }

        public string? PricingRange { get; set; }
    }

    // ==================== ADDRESS OPERATIONS ====================

    public class AddAddressRequest
    {
        [Required(ErrorMessage = "Label is required")]
        [StringLength(50, ErrorMessage = "Label cannot exceed 50 characters")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required")]
        public string District { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Postal Code cannot exceed 10 characters")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; }

        // GPS Coordinates (optional)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? AdditionalNotes { get; set; }
    }

    public class EditAddressRequest
    {
        [Required(ErrorMessage = "Label is required")]
        [StringLength(50, ErrorMessage = "Label cannot exceed 50 characters")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required")]
        public string District { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Postal Code cannot exceed 10 characters")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? AdditionalNotes { get; set; }
    }

    // ==================== SERVICE OPERATIONS ====================

    public class AddServiceRequest
    {
        [Required(ErrorMessage = "Service Name is required")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base Price is required")]
        [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100,000 EGP")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Estimated Duration is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int EstimatedDuration { get; set; }

        public string? ServiceType { get; set; }

        public bool IsCustomizable { get; set; }
    }

    public class EditServiceRequest
    {
        [Required(ErrorMessage = "Service Name is required")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base Price is required")]
        [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100,000 EGP")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Estimated Duration is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int EstimatedDuration { get; set; }

        public string? ServiceType { get; set; }

        public bool IsCustomizable { get; set; }
    }
}
