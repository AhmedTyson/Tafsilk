namespace TafsilkPlatform.Models.ViewModels.Payments
{
    public class PaymentSuccessViewModel
    {
        public Guid OrderId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TailorName { get; set; }
        public string? TailorShopName { get; set; }
    }
}
