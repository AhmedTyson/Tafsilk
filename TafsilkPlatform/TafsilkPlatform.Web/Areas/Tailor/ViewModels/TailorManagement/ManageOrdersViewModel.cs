using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Web.Areas.Tailor.ViewModels.TailorManagement;

public class ManageOrdersViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;

    // Statistics
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public double TotalRevenue { get; set; }

    // Filters
    public string? SearchTerm { get; set; }
    public OrderStatus? FilterStatus { get; set; }
    public string? DateRange { get; set; }

    // Data
    public List<OrderItemDto> Orders { get; set; } = new();
}

public class OrderItemDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty; // e.g. #ORD-1234
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public double TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsNew { get; set; } // For highlighting new orders
    public string OrderType { get; set; } = string.Empty;
}
