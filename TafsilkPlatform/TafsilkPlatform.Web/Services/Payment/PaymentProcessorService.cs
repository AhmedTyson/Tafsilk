using Microsoft.EntityFrameworkCore;
using Stripe;
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
    private readonly string? _paymentMethodConfigurationId;

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
        _paymentMethodConfigurationId = _configuration["Payment:Stripe:PaymentMethodConfigurationId"];

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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
    /// </summary>
    private async Task<PaymentProcessingResult> ProcessStripePaymentAsync(Order order, PaymentProcessingRequest request)
    {
        await Task.CompletedTask;
        // ✅ READY FOR STRIPE INTEGRATION
        // This method will handle Stripe payment processing when you add the Stripe.net NuGet package

        Logger.LogInformation("Processing Stripe payment for order {OrderId}", order.OrderId);

        // Create a pending payment record first
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
            PaidAt = default,
            Provider = "Stripe"
        };

        _context.Payment.Add(payment);

        // We don't create the PaymentIntent here anymore, the frontend will call CreatePaymentIntentAsync
        // This method is just to record the attempt and return necessary info

        return new PaymentProcessingResult
        {
            PaymentId = payment.PaymentId,
            RequiresAction = true,
            Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
            Message = "Redirecting to payment..."
        };
    }

    /// <summary>
    /// Create a Stripe Checkout Session
    /// </summary>
    public async Task<Result<string>> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateGuid(request.OrderId, nameof(request.OrderId));
            ValidatePositive(request.Amount, nameof(request.Amount));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled. Please configure Stripe in appsettings.json");
            }

            var stripe = new StripeClient(_stripeSecretKey);

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                // Use Payment Method Configuration from Dashboard (enables Cash App Pay, etc.)
                PaymentMethodConfiguration = _paymentMethodConfigurationId ?? "pmc_1SVjwXFfjWhZZwelqjN5Gyh5",

                // ✅ APPLIED FROM GUIDE: Explicitly set payment method types
                // PaymentMethodTypes = new List<string> { "card", "cashapp" },

                /*
                PaymentMethodOptions = new Stripe.Checkout.SessionPaymentMethodOptionsOptions
                {
                    Card = new Stripe.Checkout.SessionPaymentMethodOptionsCardOptions
                    {
                        SetupFutureUsage = "off", // Helps reduce "Link" prominence
                    },
                },
                */
                // PaymentMethodTypes = new List<string> { "card" }, // Removed in favor of Configuration
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(request.Amount * 100),
                Currency = request.Currency?.ToLower() ?? "usd", // Changed to USD for Cash App Pay support
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.Description ?? "Tafsilk Order",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                CustomerEmail = request.CustomerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() }
                },
                PaymentIntentData = new Stripe.Checkout.SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", request.OrderId.ToString() }
                    }
                }
            };

            var service = new Stripe.Checkout.SessionService(stripe);
            var session = await service.CreateAsync(options);

            return session.Url;
        }, "CreateCheckoutSession");
    }

    /// <summary>
    /// Create payment intent for Stripe (frontend will confirm)
    /// </summary>
    public async Task<Result<string>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            ValidateGuid(request.OrderId, nameof(request.OrderId));
            ValidatePositive(request.Amount, nameof(request.Amount));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled. Please configure Stripe in appsettings.json");
            }

            var stripe = new StripeClient(_stripeSecretKey);
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency?.ToLower() ?? "usd", // Changed to USD for Cash App Pay support
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

            var service = new PaymentIntentService(stripe);
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.ClientSecret;
        }, "CreatePaymentIntent");
    }

    /// <summary>
    /// Create a Setup Intent for future payments (e.g. Cash App Pay)
    /// </summary>
    public async Task<Result<string>> CreateSetupIntentAsync(CreateSetupIntentRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            ValidateNotEmpty(request.CustomerEmail, nameof(request.CustomerEmail));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled. Please configure Stripe in appsettings.json");
            }

            var stripe = new StripeClient(_stripeSecretKey);
            var options = new SetupIntentCreateOptions
            {
                Confirm = true,
                Usage = "off_session",
                ReturnUrl = request.ReturnUrl,
                PaymentMethodTypes = new List<string> { request.PaymentMethodType },
                PaymentMethodData = new SetupIntentPaymentMethodDataOptions
                {
                    Type = request.PaymentMethodType,
                },
                MandateData = new SetupIntentMandateDataOptions
                {
                    CustomerAcceptance = new SetupIntentMandateDataCustomerAcceptanceOptions
                    {
                        Type = "online",
                        Online = new SetupIntentMandateDataCustomerAcceptanceOnlineOptions
                        {
                            IpAddress = "127.0.0.0", // In production, get from HttpContext
                            UserAgent = "device"     // In production, get from HttpContext
                        }
                    }
                }
            };

            var service = new SetupIntentService(stripe);
            var setupIntent = await service.CreateAsync(options);

            return setupIntent.ClientSecret;
        }, "CreateSetupIntent");
    }

    /// <summary>
    /// Confirm Stripe payment from webhook
    /// </summary>
    public async Task<Result> ConfirmStripePaymentAsync(string json, string stripeSignature)
    {
        return await ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            await Task.CompletedTask;

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _stripeWebhookSecret
                );

                if (stripeEvent.Type == "payment_intent.succeeded" || stripeEvent.Type == "checkout.session.completed")
                {
                    string? orderIdStr = null;
                    long amount = 0;
                    string? currency = "";
                    string? transactionId = "";

                    if (stripeEvent.Type == "payment_intent.succeeded")
                    {
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (paymentIntent != null)
                        {
                            paymentIntent.Metadata.TryGetValue("order_id", out orderIdStr);
                            amount = paymentIntent.Amount;
                            currency = paymentIntent.Currency;
                            transactionId = paymentIntent.Id;
                        }
                    }
                    else if (stripeEvent.Type == "checkout.session.completed")
                    {
                        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                        if (session != null)
                        {
                            session.Metadata.TryGetValue("order_id", out orderIdStr);
                            amount = session.AmountTotal ?? 0;
                            currency = session.Currency;
                            transactionId = session.PaymentIntentId ?? session.Id;
                        }
                    }

                    if (!string.IsNullOrEmpty(orderIdStr) && Guid.TryParse(orderIdStr, out var orderId))
                    {
                        await _unitOfWork.ExecuteInTransactionAsync(async () =>
                        {
                            // Check if order is already paid to prevent duplicate payments from multiple events
                            var existingCompletedPayment = await _context.Payment
                                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

                            if (existingCompletedPayment != null)
                            {
                                Logger.LogInformation("Payment already completed for order {OrderId}. Skipping webhook processing.", orderId);
                                return;
                            }

                            // Find existing pending payment or create new one
                            var payment = await _context.Payment
                                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending);

                            if (payment == null)
                            {
                                // If no pending payment found, create one (this handles cases where webhook arrives before local record is created, though unlikely with our flow)
                                var order = await _context.Orders.Include(o => o.Customer).Include(o => o.Tailor).FirstOrDefaultAsync(o => o.OrderId == orderId);
                                if (order != null)
                                {
                                    payment = new TafsilkPlatform.Models.Models.Payment
                                    {
                                        PaymentId = Guid.NewGuid(),
                                        OrderId = order.OrderId,
                                        Order = order,
                                        CustomerId = order.CustomerId,
                                        Customer = order.Customer,
                                        TailorId = order.TailorId,
                                        Tailor = order.Tailor,
                                        Amount = (decimal)amount / 100m,
                                        PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Card,
                                        TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
                                        Provider = "Stripe",
                                        CreatedAt = DateTimeOffset.UtcNow
                                    };
                                    _context.Payment.Add(payment);
                                }
                            }

                            if (payment != null)
                            {
                                payment.PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed;
                                payment.PaidAt = DateTimeOffset.UtcNow;
                                payment.ProviderTransactionId = transactionId;
                                payment.Currency = currency;

                                // Update order status
                                var order = await _context.Orders.FindAsync(payment.OrderId);
                                if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.PendingPayment))
                                {
                                    order.Status = OrderStatus.Confirmed;

                                    // Clear cart now that payment is confirmed
                                    var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(order.CustomerId);
                                    if (cart != null)
                                    {
                                        await _unitOfWork.ShoppingCarts.ClearCartAsync(cart.CartId);
                                    }
                                }

                                await _unitOfWork.SaveChangesAsync();
                                Logger.LogInformation("Payment confirmed via webhook for order {OrderId}", orderId);
                            }
                        });
                    }
                }
            }
            catch (StripeException e)
            {
                Logger.LogError(e, "Stripe webhook error");
                throw;
            }
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

                // If Stripe payment, process refund via Stripe
                if (payment.Provider == "Stripe" && !string.IsNullOrEmpty(payment.ProviderTransactionId) && _stripeEnabled)
                {
                    var stripe = new StripeClient(_stripeSecretKey);
                    var refundService = new RefundService(stripe);
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = payment.ProviderTransactionId,
                        Amount = (long)(amount * 100),
                        Reason = RefundReasons.RequestedByCustomer // Or map from reason string
                    };
                    await refundService.CreateAsync(refundOptions);
                }

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

    /// <summary>
    /// Verify a Checkout Session and update payment status if paid
    /// </summary>
    public async Task<Result<bool>> VerifyCheckoutSessionAsync(string sessionId)
    {
        return await ExecuteAsync(async () =>
        {
            ValidateNotEmpty(sessionId, nameof(sessionId));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled");
            }

            var stripe = new StripeClient(_stripeSecretKey);
            var service = new Stripe.Checkout.SessionService(stripe);
            var session = await service.GetAsync(sessionId);

            if (session == null)
            {
                throw new InvalidOperationException("Session not found");
            }

            if (session.PaymentStatus == "paid")
            {
                // Extract order ID from metadata
                if (session.Metadata.TryGetValue("order_id", out var orderIdStr) && Guid.TryParse(orderIdStr, out var orderId))
                {
                    await _unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        // Check if already completed to avoid redundant updates
                        var existingCompletedPayment = await _context.Payment
                            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

                        if (existingCompletedPayment != null)
                        {
                            return; // Already processed
                        }

                        // Find pending payment or create new one
                        var payment = await _context.Payment
                            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending);

                        if (payment == null)
                        {
                            var order = await _context.Orders.Include(o => o.Customer).Include(o => o.Tailor).FirstOrDefaultAsync(o => o.OrderId == orderId);
                            if (order != null)
                            {
                                payment = new TafsilkPlatform.Models.Models.Payment
                                {
                                    PaymentId = Guid.NewGuid(),
                                    OrderId = order.OrderId,
                                    Order = order,
                                    CustomerId = order.CustomerId,
                                    Customer = order.Customer,
                                    TailorId = order.TailorId,
                                    Tailor = order.Tailor,
                                    Amount = (decimal)(session.AmountTotal ?? 0) / 100m,
                                    PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Card,
                                    TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
                                    Provider = "Stripe",
                                    CreatedAt = DateTimeOffset.UtcNow
                                };
                                _context.Payment.Add(payment);
                            }
                        }

                        if (payment != null)
                        {
                            payment.PaymentStatus = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed;
                            payment.PaidAt = DateTimeOffset.UtcNow;
                            payment.ProviderTransactionId = session.PaymentIntentId ?? session.Id;
                            payment.Currency = session.Currency;

                            // Update order status
                            var order = await _context.Orders.FindAsync(payment.OrderId);
                            if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.PendingPayment))
                            {
                                order.Status = OrderStatus.Confirmed;

                                // Clear cart now that payment is confirmed
                                var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(order.CustomerId);
                                if (cart != null)
                                {
                                    await _unitOfWork.ShoppingCarts.ClearCartAsync(cart.CartId);
                                }
                            }

                            await _unitOfWork.SaveChangesAsync();
                            Logger.LogInformation("Payment verified and confirmed via Session Check for order {OrderId}", orderId);
                        }
                    });

                    return true;
                }
            }

            return false;
        }, "VerifyCheckoutSession");
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
