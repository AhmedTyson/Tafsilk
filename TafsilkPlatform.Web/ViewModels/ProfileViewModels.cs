using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels
{
    // ==================== CUSTOMER PROFILE ====================

    public class UpdateCustomerProfileRequest
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        [RegularExpression(@"^01[0-2,5]\d{8}$", ErrorMessage = "رقم هاتف مصري غير صحيح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "الجنس مطلوب")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        [StringLength(500, ErrorMessage = "التفضيلات لا يمكن أن تتجاوز 500 حرف")]
        public string? Preferences { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "النبذة لا يمكن أن تتجاوز 500 حرف")]
        public string? Bio { get; set; }
    }

    // ==================== TAILOR PROFILE ====================

    public class UpdateTailorProfileRequest
    {
        [Required(ErrorMessage = "اسم المحل مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "اسم المحل يجب أن يكون بين 3 و 100 حرف")]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "النبذة مطلوبة")]
        [StringLength(500, ErrorMessage = "النبذة لا يمكن أن تتجاوز 500 حرف")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        [RegularExpression(@"^01[0-2,5]\d{8}$", ErrorMessage = "رقم هاتف مصري غير صحيح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(255, ErrorMessage = "العنوان لا يمكن أن يتجاوز 255 حرف")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        [Range(0, 60, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 60")]
        public int? ExperienceYears { get; set; }

        public string? SkillLevel { get; set; }

        public string? PricingRange { get; set; }
    }

    // ==================== ADDRESS OPERATIONS ====================

    public class AddAddressRequest
    {
        [Required(ErrorMessage = "تسمية العنوان مطلوبة")]
        [StringLength(50, ErrorMessage = "التسمية لا يمكن أن تتجاوز 50 حرف")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(255, ErrorMessage = "العنوان لا يمكن أن يتجاوز 255 حرف")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "الحي مطلوب")]
        public string District { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "الرمز البريدي لا يمكن أن يتجاوز 10 أحرف")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; }

        // GPS Coordinates (optional)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? AdditionalNotes { get; set; }
    }

    public class EditAddressRequest
    {
        [Required(ErrorMessage = "تسمية العنوان مطلوبة")]
        [StringLength(50, ErrorMessage = "التسمية لا يمكن أن تتجاوز 50 حرف")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(255, ErrorMessage = "العنوان لا يمكن أن يتجاوز 255 حرف")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "الحي مطلوب")]
        public string District { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "الرمز البريدي لا يمكن أن يتجاوز 10 أحرف")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? AdditionalNotes { get; set; }
    }

    // ==================== SERVICE OPERATIONS ====================

    public class AddServiceRequest
    {
        [Required(ErrorMessage = "اسم الخدمة مطلوب")]
        [StringLength(100, ErrorMessage = "اسم الخدمة لا يمكن أن يتجاوز 100 حرف")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر الأساسي مطلوب")]
        [Range(1, 100000, ErrorMessage = "السعر يجب أن يكون بين 1 و 100,000 جنيه")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "المدة التقديرية مطلوبة")]
        [Range(1, 365, ErrorMessage = "المدة يجب أن تكون بين 1 و 365 يوم")]
        public int EstimatedDuration { get; set; }

        public string? ServiceType { get; set; }

        public bool IsCustomizable { get; set; }
    }

    public class EditServiceRequest
    {
        [Required(ErrorMessage = "اسم الخدمة مطلوب")]
        [StringLength(100, ErrorMessage = "اسم الخدمة لا يمكن أن يتجاوز 100 حرف")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر الأساسي مطلوب")]
        [Range(1, 100000, ErrorMessage = "السعر يجب أن يكون بين 1 و 100,000 جنيه")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "المدة التقديرية مطلوبة")]
        [Range(1, 365, ErrorMessage = "المدة يجب أن تكون بين 1 و 365 يوم")]
        public int EstimatedDuration { get; set; }

        public string? ServiceType { get; set; }

        public bool IsCustomizable { get; set; }
    }
}
