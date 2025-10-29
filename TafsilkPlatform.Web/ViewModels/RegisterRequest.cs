namespace TafsilkPlatform.Web.ViewModels
{
    public enum RegistrationRole
    {
        Customer,
        Tailor,
        Corporate
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public RegistrationRole Role { get; set; }
        // Optional profile fields
        public string? FullName { get; set; }
        public string? ShopName { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactPerson { get; set; }
    }
}
