namespace TafsilkPlatform.Web.ViewModels.Orders;

/// <summary>
/// Result DTO for order creation operations
/// Used for idempotency and consistent responses
/// </summary>
public class OrderResult
{
    public bool Success { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public OrderSummaryDto? Order { get; set; }
}

/// <summary>
/// Lightweight order summary for response
/// </summary>
public class OrderSummaryDto
{
  public Guid OrderId { get; set; }
public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string TailorName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}
