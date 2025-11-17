namespace TafsilkPlatform.Shared.Models
{
    /// <summary>
    /// Shared user profile model used across both projects
    /// </summary>
    public class UserProfile
  {
        public Guid Id { get; set; }
     public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
     public string Role { get; set; } = string.Empty; // Customer, Tailor, Admin
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Shared tailor profile model
    /// </summary>
    public class TailorProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
 public string ShopName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Specialties { get; set; } = new();
        public DateTime CreatedAt { get; set; }
 public DateTime? UpdatedAt { get; set; }
 }

    /// <summary>
    /// Shared customer profile model
/// </summary>
    public class CustomerProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string City { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// Shared service model
    /// </summary>
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public Guid TailorId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
   public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int EstimatedDays { get; set; }
public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Shared order model
    /// </summary>
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid TailorId { get; set; }
     public string TailorName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    /// <summary>
    /// Shared address model
    /// </summary>
    public class AddressDto
    {
  public Guid Id { get; set; }
        public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? BuildingNumber { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
