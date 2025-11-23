using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Models.ViewModels.Orders;

public class OrderDetailsViewModel
{
    // Order Information
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public decimal TotalPrice { get; set; }

    // Customer Information
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    // Tailor Information
    public Guid TailorId { get; set; }
    public string? TailorName { get; set; }
    public string? TailorShopName { get; set; }
    public string? TailorPhone { get; set; }
    public byte[]? TailorProfilePictureData { get; set; }
    public string? TailorProfilePictureContentType { get; set; }

    // Order Items
    public List<OrderItemViewModel> Items { get; set; } = new();

    // Payment Information
    public bool IsPaid { get; set; }
    public decimal PaymentAmount { get; set; }

    // Images
    public List<OrderImageViewModel> ReferenceImages { get; set; } = new();

    // User Context
    public bool IsCustomer { get; set; }
    public bool IsTailor { get; set; }

    // Status Timeline
    public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new();
}

public class OrderItemViewModel
{
    public Guid ItemId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Notes { get; set; }
}

public class OrderImageViewModel
{
    public Guid ImageId { get; set; }
    public string? ContentType { get; set; }
    public string? ImgUrl { get; set; }
}

public class OrderStatusHistoryViewModel
{
    public OrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string? Notes { get; set; }
}
