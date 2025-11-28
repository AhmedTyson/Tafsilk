using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels;

public class CompleteGoogleRegistrationViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Account Type")]
    public string UserType { get; set; } = "customer";

    public string? ProfilePictureUrl { get; set; }
}
