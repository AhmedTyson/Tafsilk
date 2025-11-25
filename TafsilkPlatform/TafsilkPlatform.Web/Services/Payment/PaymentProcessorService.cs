using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Web.Common;
using TafsilkPlatform.Web.Services.Base;

namespace TafsilkPlatform.Web.Services.Payment;

/// <summary>
/// Payment processor service with support for Cash and Stripe
/// Stripe integration is configured via appsettings.json and can be enabled when ready
/// </summary>
public class PaymentProcessorService : BaseService, IPaymentProcessorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly bool _stripeEnabled;
    private readonly string? _stripeSecretKey;
    private readonly string? _stripeWebhookSecret;

    public PaymentProcessorService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<PaymentProcessorService> logger) : base(logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Check if Stripe is enabled in configuration
        _stripeEnabled = _configuration.GetValue<bool>("Payment:Stripe:Enabled", false);
        _stripeSecretKey = _configuration["Payment:Stripe:SecretKey"];
        _stripeWebhookSecret = _configuration["Payment:Stripe:WebhookSecret"];

        if (_stripeEnabled && string.IsNullOrEmpty(_stripeSecretKey))
        {
            Logger.LogWarning("Stripe is enabled but SecretKey is not configured. Falling back to cash-only mode.");
            _stripeEnabled = false;
        }
    }

    /// <summary>
    /// Process payment for an order
    /// Supports: Cash on Delivery, Credit Card (future), Stripe
    /// </summary>
    public async Task<Result<PaymentProcessingResult>> ProcessPaymentAsync(PaymentProcessingRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            // Validation
            ValidateGuid(request.OrderId, nameof(request.OrderId));
            ValidateGuid(request.CustomerId, nameof(request.CustomerId));
            ValidatePositive(request.Amount, nameof(request.Amount));
            ValidateNotEmpty(request.PaymentMethod, nameof(request.PaymentMethod));

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Get order
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

                if (order == null)
                {
                    throw new InvalidOperationException("Order not found");
                }

                // Check if order already has a completed payment
                var existingPayment = await _context.Payment
                    .FirstOrDefaultAsync(p => p.OrderId == request.OrderId &&
                                            p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

                if (existingPayment != null)
                {
                    throw new InvalidOperationException("Order already has a completed payment");
                }

                // Validate amount matches order total
                if (Math.Abs((decimal)order.TotalPrice - request.Amount) > 0.01m)
                {
                    throw new InvalidOperationException($"Payment amount ({request.Amount:C}) does not match order total ({order.TotalPrice:C})");
                }

                PaymentProcessingResult result;

                // Process based on payment method
                switch (request.PaymentMethod.ToLower())
                {
                    case "cashondelivery":
                    case "cash":
                        result = await ProcessCashPaymentAsync(order, request);
                        break;

                    case "creditcard":
                    case "stripe":
                        if (_stripeEnabled)
                        {
                            result = await ProcessStripePaymentAsync(order, request);
                        }
                        else
                        {
                            // Fallback: Mark as pending, will be processed when Stripe is configured
                            Logger.LogWarning("Stripe not configured. Creating pending payment for order {OrderId}", request.OrderId);
                            result = await ProcessPendingCardPaymentAsync(order, request);
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported payment method: {request.PaymentMethod}");
                }

                await _unitOfWork.SaveChangesAsync();

                Logger.LogInformation(
                    "Payment processed for order {OrderId}. Method: {PaymentMethod}, Status: {Status}",
                    request.OrderId, request.PaymentMethod, result.Status);

                return result;
            });
        }, "ProcessPayment", request.CustomerId);
    }

    /// <summary>
    /// Process cash on delivery payment
    /// Payment is marked as Pending until delivery
    /// </summary>
    private async Task<PaymentProcessingResult> ProcessCashPaymentAsync(Order order, PaymentProcessingRequest request)
    {
        var payment = new TafsilkPlatform.Models.Models.Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Order = order,
            CustomerId = request.CustomerId,
            Customer = order.Customer,
            TailorId = order.TailorId,
            Tailor = order.Tailor,
            Amount = request.Amount,
            PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Cash,
            PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending, // Will be completed on delivery
            TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
            PaidAt = default // Not paid yet
        };

        _context.Payment.Add(payment);

        // Update order status
        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Confirmed;
        }

        return new PaymentProcessingResult
        {
            PaymentId = payment.PaymentId,
            RequiresAction = false,
            Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            Message = "Order confirmed. Payment will be collected on delivery."
        };
    }

    /// <summary>
    /// Process pending card payment (when Stripe is not configured)
    /// </summary>
    private async Task<PaymentProcessingResult> ProcessPendingCardPaymentAsync(Order order, PaymentProcessingRequest request)
    {
        var payment = new TafsilkPlatform.Models.Models.Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Order = order,
            CustomerId = request.CustomerId,
            Customer = order.Customer,
            TailorId = order.TailorId,
            Tailor = order.Tailor,
            Amount = request.Amount,
            PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Card,
            PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
            PaidAt = default
        };

        _context.Payment.Add(payment);

        return new PaymentProcessingResult
        {
            PaymentId = payment.PaymentId,
            RequiresAction = true,
            Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            Message = "Payment pending. Card processing will be available soon."
        };
    }

    /// <summary>
    /// Process Stripe payment
    /// TO BE IMPLEMENTED: When Stripe SDK is added
    /// </summary>
    private async Task<PaymentProcessingResult> ProcessStripePaymentAsync(Order order, PaymentProcessingRequest request)
    {
        // ✅ READY FOR STRIPE INTEGRATION
        // This method will handle Stripe payment processing when you add the Stripe.net NuGet package

        Logger.LogInformation("Processing Stripe payment for order {OrderId}", order.OrderId);

        // TODO: When Stripe is integrated, uncomment and implement:
        /*
        var stripe = new StripeClient(_stripeSecretKey);
        
        var paymentIntentCreateOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Stripe uses smallest currency unit (halalas for SAR)
            Currency = request.Currency?.ToLower() ?? "sar",
            PaymentMethod = request.StripePaymentMethodId,
            Customer = request.StripeCustomerId,
            Confirm = true, // Confirm immediately
            ReturnUrl = $"{_configuration["AppSettings:BaseUrl"]}/orders/{order.OrderId}",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.OrderId.ToString() },
                { "customer_id", request.CustomerId.ToString() }
            }
        };

        var paymentIntent = await stripe.PaymentIntents.CreateAsync(paymentIntentCreateOptions);
        */

        // For now, create a pending payment record
        var payment = new TafsilkPlatform.Models.Models.Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Order = order,
            CustomerId = request.CustomerId,
            Customer = order.Customer,
            TailorId = order.TailorId,
            Tailor = order.Tailor,
            Amount = request.Amount,
            PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Card,
            PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
            PaidAt = default
        };

        _context.Payment.Add(payment);

        return new PaymentProcessingResult
        {
            PaymentId = payment.PaymentId,
            RequiresAction = true, // Will require 3D Secure when Stripe is integrated
            ClientSecret = null, // Will be: paymentIntent.ClientSecret
            Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            ProviderTransactionId = null, // Will be: paymentIntent.Id
            Message = "Stripe payment processing ready for integration"
        };

        // When Stripe is integrated, handle different statuses:
        /*
        return paymentIntent.Status switch
        {
            "succeeded" => new PaymentProcessingResult
            {
                PaymentId = payment.PaymentId,
                RequiresAction = false,
                Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed,
                ProviderTransactionId = paymentIntent.Id,
                Message = "Payment successful"
            },
            "requires_action" => new PaymentProcessingResult
            {
                PaymentId = payment.PaymentId,
                RequiresAction = true,
                ClientSecret = paymentIntent.ClientSecret,
                RedirectUrl = paymentIntent.NextAction?.RedirectToUrl?.Url,
                Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
                ProviderTransactionId = paymentIntent.Id,
                Message = "3D Secure authentication required"
            },
            _ => throw new InvalidOperationException($"Unexpected payment status: {paymentIntent.Status}")
        };
        */
    }

    /// <summary>
    /// Create payment intent for Stripe (frontend will confirm)
    /// </summary>
    public async Task<Result<string>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateGuid(request.OrderId, nameof(request.OrderId));
            ValidatePositive(request.Amount, nameof(request.Amount));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled. Please configure Stripe in appsettings.json");
            }

            // ✅ READY FOR STRIPE INTEGRATION
            // When Stripe.net is added:
            /*
            var stripe = new StripeClient(_stripeSecretKey);
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() },
                    { "description", request.Description ?? "Tafsilk Order" }
                }
            };

            var paymentIntent = await stripe.PaymentIntents.CreateAsync(options);
            return paymentIntent.ClientSecret;
            */

            // Placeholder for now
            Logger.LogWarning("Stripe CreatePaymentIntent called but not configured");
            return "placeholder_client_secret_configure_stripe";
        }, "CreatePaymentIntent");
    }

    /// <summary>
    /// Confirm Stripe payment from webhook
    /// </summary>
    public async Task<Result> ConfirmStripePaymentAsync(string paymentIntentId, string stripeSignature)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateNotEmpty(paymentIntentId, nameof(paymentIntentId));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled");
            }

            // ✅ READY FOR STRIPE WEBHOOK INTEGRATION
            // When Stripe.net is added:
            /*
            var stripeEvent = EventUtility.ConstructEvent(
                await Request.Body.ReadAsStringAsync(),
                stripeSignature,
                _stripeWebhookSecret
            );

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                var orderId = Guid.Parse(paymentIntent.Metadata["order_id"]);

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var payment = await _context.Payment
                        .FirstOrDefaultAsync(p => p.ProviderTransactionId == paymentIntent.Id);

                    if (payment != null)
                    {
                        payment.PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed;
                        payment.PaidAt = DateTimeOffset.UtcNow;

                        // Update order status
                        var order = await _context.Orders.FindAsync(payment.OrderId);
                        if (order != null && order.Status == OrderStatus.Pending)
                        {
                            order.Status = OrderStatus.Confirmed;
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }
                });
            }
            */

            Logger.LogWarning("Stripe webhook received but not configured: {PaymentIntentId}", paymentIntentId);
        }, "ConfirmStripePayment");
    }

    /// <summary>
    /// Process refund
    /// </summary>
    public async Task<Result<RefundResult>> ProcessRefundAsync(Guid paymentId, decimal amount, string reason)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateGuid(paymentId, nameof(paymentId));
            ValidatePositive(amount, nameof(amount));
            ValidateNotEmpty(reason, nameof(reason));

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var payment = await _context.Payment
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }

                if (payment.PaymentStatus != TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed)
                {
                    throw new InvalidOperationException("Can only refund completed payments");
                }

                if (amount > payment.Amount)
                {
                    throw new InvalidOperationException($"Refund amount ({amount:C}) cannot exceed payment amount ({payment.Amount:C})");
                }

                // Create refund record
                // Note: You'll need to add a Refund table/model for this
                Logger.LogInformation("Processing refund for payment {PaymentId}. Amount: {Amount}, Reason: {Reason}",
                    paymentId, amount, reason);

                // Update payment status
                if (amount >= payment.Amount)
                {
                    payment.PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Refunded;
                }
                else
                {
                    payment.PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.PartiallyPaid;
                }

                // Update order status
                if (payment.Order != null)
                {
                    payment.Order.Status = OrderStatus.Cancelled;
                }

                await _unitOfWork.SaveChangesAsync();

                return new RefundResult
                {
                    RefundId = Guid.NewGuid(),
                    Amount = amount,
                    Status = Enums.RefundStatus.Completed,
                    Message = "Refund processed successfully"
                };
            });
        }, "ProcessRefund");
    }

    /// <summary>
    /// Get payment status
    /// </summary>
    public async Task<Result<PaymentStatusResult>> GetPaymentStatusAsync(Guid paymentId)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateGuid(paymentId, nameof(paymentId));

            var payment = await _context.Payment
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            return new PaymentStatusResult
            {
                PaymentId = payment.PaymentId,
                Status = payment.PaymentStatus,
                Amount = payment.Amount,
                PaidAt = payment.PaidAt != default ? payment.PaidAt : null,
                Message = GetPaymentStatusMessage(payment.PaymentStatus)
            };
        }, "GetPaymentStatus");
    }

    /// <summary>
    /// Validate payment before processing
    /// </summary>
    public async Task<Result> ValidatePaymentAsync(Guid orderId, decimal amount)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateGuid(orderId, nameof(orderId));
            ValidatePositive(amount, nameof(amount));

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found");
            }

            if (Math.Abs((decimal)order.TotalPrice - amount) > 0.01m)
            {
                throw new InvalidOperationException($"Amount mismatch. Expected: {order.TotalPrice:C}, Provided: {amount:C}");
            }

            // Check if order already paid
            var existingPayment = await _context.Payment
                .FirstOrDefaultAsync(p => p.OrderId == orderId &&
                                        p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

            if (existingPayment != null)
            {
                throw new InvalidOperationException("Order already paid");
            }
        }, "ValidatePayment");
    }

    private string GetPaymentStatusMessage(TafsilkPlatform.Models.Models.Enums.PaymentStatus status) => status switch
    {
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending => "Payment pending",
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed => "Payment completed successfully",
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.Failed => "Payment failed",
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.Refunded => "Payment refunded",
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.Cancelled => "Payment cancelled",
        TafsilkPlatform.Models.Models.Enums.PaymentStatus.PartiallyPaid => "Payment partially completed",
        _ => "Unknown status"
    };
}
