namespace TafsilkPlatform.MVC.Models
{
    /// <summary>
    /// User model for authentication
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
     public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
 public string Role { get; set; } = string.Empty; // "Customer", "Tailor", "Admin"
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
