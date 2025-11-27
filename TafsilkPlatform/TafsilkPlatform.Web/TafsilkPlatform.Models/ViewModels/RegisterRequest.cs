using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    public RegistrationRole Role { get; set; } = RegistrationRole.Customer;

    // Tailor-specific fields
    public string? ShopName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}
