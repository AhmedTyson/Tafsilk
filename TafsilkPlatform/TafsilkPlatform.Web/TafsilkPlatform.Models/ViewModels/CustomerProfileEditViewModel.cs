using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels
{
    public class CustomerProfileEditViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public string? Email { get; set; }

        // Read-only statistics
        public DateTime MemberSince { get; set; }
        public int TotalOrders { get; set; }
    }
}
