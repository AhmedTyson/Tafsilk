using TafsilkPlatform.Web.Common;

namespace TafsilkPlatform.Web.Services.Payment;

/// <summary>
/// Interface for payment processing services
/// Supports multiple payment providers (Cash, Stripe, etc.)
/// </summary>
public interface IPaymentProcessorService
{
    /// <summary>
    /// Process payment for an order
    /// </summary>
    Task<Result<PaymentProcessingResult>> ProcessPaymentAsync(PaymentProcessingRequest request);

    /// <summary>
    /// Create a payment intent for Stripe (for future integration)
    /// </summary>
    Task<Result<string>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);

    /// <summary>
    /// Confirm Stripe payment (webhook handler)
    /// </summary>
    Task<Result> ConfirmStripePaymentAsync(string paymentIntentId, string stripeSignature);

    /// <summary>
    /// Process refund
    /// </summary>
    Task<Result<RefundResult>> ProcessRefundAsync(Guid paymentId, decimal amount, string reason);

    /// <summary>
    /// Get payment status
    /// </summary>
    Task<Result<PaymentStatusResult>> GetPaymentStatusAsync(Guid paymentId);

    /// <summary>
    /// Validate payment before processing
    /// </summary>
    Task<Result> ValidatePaymentAsync(Guid orderId, decimal amount);
}

/// <summary>
/// Payment processing request
/// </summary>
public class PaymentProcessingRequest
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "CashOnDelivery"; // CashOnDelivery, CreditCard, Stripe
    public string? Currency { get; set; } = "SAR";
    
    // Stripe-specific fields
    public string? StripePaymentMethodId { get; set; }
    public string? StripeCustomerId { get; set; }
    
    // Shipping info
    public string? ShippingStreet { get; set; }
    public string? ShippingCity { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}

/// <summary>
/// Payment processing result
/// </summary>
public class PaymentProcessingResult
{
    public Guid PaymentId { get; set; }
    public bool RequiresAction { get; set; } // For Stripe 3D Secure
    public string? ClientSecret { get; set; } // For Stripe confirmation
    public string? RedirectUrl { get; set; } // For 3D Secure redirect
    public Models.Enums.PaymentStatus Status { get; set; }
    public string? ProviderTransactionId { get; set; } // Stripe PaymentIntent ID
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Create payment intent request (Stripe)
/// </summary>
public class CreatePaymentIntentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string CustomerEmail { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Refund result
/// </summary>
public class RefundResult
{
    public Guid RefundId { get; set; }
    public decimal Amount { get; set; }
    public string? ProviderRefundId { get; set; } // Stripe refund ID
    public Models.Enums.RefundStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Payment status result
/// </summary>
public class PaymentStatusResult
{
    public Guid PaymentId { get; set; }
    public Models.Enums.PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public decimal? RefundedAmount { get; set; }
    public string? ProviderTransactionId { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
