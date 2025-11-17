namespace TafsilkPlatform.MVC.Models
{
    // ==================== MOCK DATA MODELS ====================
    
    public class TailorProfile
    {
        public Guid Id { get; set; }
 public string ShopName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
     public string Bio { get; set; } = string.Empty;
  public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Specialties { get; set; } = new();
    }

    public class CustomerProfile
    {
    public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
      public string PhoneNumber { get; set; } = string.Empty;
 public string City { get; set; } = string.Empty;
      public int OrderCount { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class TailorService
    {
        public Guid Id { get; set; }
 public Guid TailorId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
  public int EstimatedDays { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class Order
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

    public class DashboardStats
    {
     public int TotalOrders { get; set; }
   public int PendingOrders { get; set; }
 public int CompletedOrders { get; set; }
      public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalTailors { get; set; }
    }
}
