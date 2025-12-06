namespace TafsilkPlatform.Models.ViewModels.Store
{
    /// <summary>
    /// View model for payment success page
    /// </summary>
    public class PaymentSuccessViewModel
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; }
        public int EstimatedDeliveryDays { get; set; }

        // ✅ NEW: Support for multiple orders
        public List<OrderSuccessDetailsViewModel> Orders { get; set; } = new();
    }

    /// <summary>
    /// View model for order details after checkout
    /// </summary>
    public class OrderSuccessDetailsViewModel
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? TailorName { get; set; }
        public string? TailorShopName { get; set; }
        public List<OrderSuccessItemViewModel> Items { get; set; } = new();
    }

    /// <summary>
    /// Order item for success page
    /// </summary>
    public class OrderSuccessItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
