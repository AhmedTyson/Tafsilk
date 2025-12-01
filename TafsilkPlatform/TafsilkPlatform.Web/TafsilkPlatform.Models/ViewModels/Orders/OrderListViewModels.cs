using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Models.ViewModels.Orders;

public class CustomerOrdersViewModel
{
    public List<OrderSummaryViewModel> Orders { get; set; } = new();
    public int TotalOrders => Orders.Count;
    public int PendingOrders => Orders.Count(o => o.Status == OrderStatus.Pending);
    public int ProcessingOrders => Orders.Count(o => o.Status == OrderStatus.Processing);
    public int CompletedOrders => Orders.Count(o => o.Status == OrderStatus.Delivered);
}

public class TailorOrdersViewModel
{
    public List<OrderSummaryViewModel> Orders { get; set; } = new();
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class OrderSummaryViewModel
{
    public Guid OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? TailorName { get; set; }
    public string? TailorShopName { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
