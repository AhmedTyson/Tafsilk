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
            // Ensure OrderIds list is populated
            if (!request.OrderIds.Any() && request.OrderId != Guid.Empty)
            {
                request.OrderIds.Add(request.OrderId);
            }

            // Validation
            if (!request.OrderIds.Any()) throw new ArgumentException("No orders specified", nameof(request.OrderIds));
            ValidateGuid(request.CustomerId, nameof(request.CustomerId));
            ValidatePositive(request.Amount, nameof(request.Amount));
            ValidateNotEmpty(request.PaymentMethod, nameof(request.PaymentMethod));

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var orders = new List<Order>();
                decimal totalOrderAmount = 0;

                foreach (var orderId in request.OrderIds)
                {
                    var order = await _context.Orders
                        .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order == null) throw new InvalidOperationException($"Order {orderId} not found");

                    // Check if order already has a completed payment
                    var existingPayment = await _context.Payment
                        .FirstOrDefaultAsync(p => p.OrderId == orderId &&
                                                p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

                    if (existingPayment != null) throw new InvalidOperationException($"Order {orderId} already has a completed payment");

                    orders.Add(order);
                    totalOrderAmount += (decimal)order.TotalPrice;
                }

                // Validate amount matches total of all orders
                if (Math.Abs(totalOrderAmount - request.Amount) > 0.01m)
                {
                    throw new InvalidOperationException($"Payment amount ({request.Amount:C}) does not match total of orders ({totalOrderAmount:C})");
                }

                PaymentProcessingResult result = new PaymentProcessingResult();

                // Process based on payment method
                switch (request.PaymentMethod.ToLower())
                {
                    case "cashondelivery":
                    case "cash":
                        foreach (var order in orders)
                        {
                            // Create request for single order
                            var singleRequest = new PaymentProcessingRequest
                            {
                                OrderId = order.OrderId,
                                CustomerId = request.CustomerId,
                                Amount = (decimal)order.TotalPrice,
                                PaymentMethod = request.PaymentMethod
                            };
                            result = await ProcessCashPaymentAsync(order, singleRequest);
                        }
                        // Return result for the last one (or aggregated)
                        result.Message = $"Orders confirmed. Payment will be collected on delivery.";
                        break;

                    case "creditcard":
                    case "stripe":
                        if (_stripeEnabled)
                        {
                            // For Stripe, we create one payment intent/session for ALL orders
                            // But we need to record pending payments for EACH order
                            foreach (var order in orders)
                            {
                                var singleRequest = new PaymentProcessingRequest
                                {
                                    OrderId = order.OrderId,
                                    CustomerId = request.CustomerId,
                                    Amount = (decimal)order.TotalPrice,
                                    PaymentMethod = request.PaymentMethod
                                };
                                result = await ProcessStripePaymentAsync(order, singleRequest);
                            }
                            result = new PaymentProcessingResult
                            {
                                RequiresAction = true,
                                Status = TafsilkPlatform.Models.Models.Enums.PaymentStatus.Pending,
                                Message = "Redirecting to payment..."
                            };
                        }
                        else
                        {
                            foreach (var order in orders)
                            {
                                var singleRequest = new PaymentProcessingRequest
                                {
                                    OrderId = order.OrderId,
                                    CustomerId = request.CustomerId,
                                    Amount = (decimal)order.TotalPrice,
                                    PaymentMethod = request.PaymentMethod
                                };
                                result = await ProcessPendingCardPaymentAsync(order, singleRequest);
                            }
                            result.Message = "Payments pending. Card processing will be available soon.";
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported payment method: {request.PaymentMethod}");
                }

                await _unitOfWork.SaveChangesAsync();

                Logger.LogInformation(
                    "Payment processed for {OrderCount} orders. Method: {PaymentMethod}, Status: {Status}",
                    orders.Count, request.PaymentMethod, result.Status);

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
            // Ensure OrderIds list is populated
            if (!request.OrderIds.Any() && request.OrderId != Guid.Empty)
            {
                request.OrderIds.Add(request.OrderId);
            }

            if (!request.OrderIds.Any()) throw new ArgumentException("No orders specified", nameof(request.OrderIds));
            ValidatePositive(request.Amount, nameof(request.Amount));

            if (!_stripeEnabled)
            {
                throw new InvalidOperationException("Stripe is not enabled. Please configure Stripe in appsettings.json");
            }

            var stripe = new StripeClient(_stripeSecretKey);

            // Create comma-separated list of Order IDs for metadata
            var orderIdsString = string.Join(",", request.OrderIds);

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                // Use Payment Method Configuration from Dashboard (enables Cash App Pay, etc.)
                PaymentMethodConfiguration = _paymentMethodConfigurationId ?? "pmc_1SVjwXFfjWhZZwelqjN5Gyh5",

                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(request.Amount * 100),
                            Currency = request.Currency?.ToLower() ?? "egp", // Changed to EGP
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.Description ?? $"Order Payment ({request.OrderIds.Count} orders)",
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
                    { "order_ids", orderIdsString }
                },
                PaymentIntentData = new Stripe.Checkout.SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_ids", orderIdsString }
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
                Currency = request.Currency?.ToLower() ?? "egp", // Changed to EGP
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
    public async Task<Result> ConfirmStripePaymentAsync(string json, string? stripeSignature)
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

                    string? orderIdsStr = null;
                    if (!string.IsNullOrEmpty(orderIdStr))
                    {
                        // Fallback for old metadata key
                        orderIdsStr = orderIdStr;
                    }
                    else
                    {
                        // Check for new metadata key
                        if (stripeEvent.Type == "payment_intent.succeeded")
                        {
                            var pi = stripeEvent.Data.Object as PaymentIntent;
                            pi?.Metadata.TryGetValue("order_ids", out orderIdsStr);
                        }
                        else if (stripeEvent.Type == "checkout.session.completed")
                        {
                            var s = stripeEvent.Data.Object as Stripe.Checkout.Session;
                            s?.Metadata.TryGetValue("order_ids", out orderIdsStr);
                        }
                    }

                    if (!string.IsNullOrEmpty(orderIdsStr))
                    {
                        var orderIds = orderIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                                  .Where(g => g != Guid.Empty)
                                                  .ToList();

                        foreach (var orderId in orderIds)
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
                                    // If no pending payment found, create one
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
                                            Amount = (decimal)order.TotalPrice, // Use order total, not full transaction amount
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
    public async Task<Result> ValidatePaymentAsync(List<Guid> orderIds, decimal amount)
    {
        return await ExecuteAsync(async () =>
        {
            if (!orderIds.Any()) throw new ArgumentException("No orders specified", nameof(orderIds));
            ValidatePositive(amount, nameof(amount));

            decimal totalCalculated = 0;

            foreach (var orderId in orderIds)
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) throw new InvalidOperationException($"Order {orderId} not found");

                // Check if order already paid
                var existingPayment = await _context.Payment
                    .FirstOrDefaultAsync(p => p.OrderId == orderId &&
                                            p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);

                if (existingPayment != null) throw new InvalidOperationException($"Order {orderId} already paid");

                totalCalculated += (decimal)order.TotalPrice;
            }

            if (Math.Abs(totalCalculated - amount) > 0.01m)
            {
                throw new InvalidOperationException($"Amount mismatch. Expected: {totalCalculated:C}, Provided: {amount:C}");
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
                // Extract order IDs from metadata
                string? orderIdsStr = null;
                if (session.Metadata.TryGetValue("order_ids", out var ids))
                {
                    orderIdsStr = ids;
                }
                else if (session.Metadata.TryGetValue("order_id", out var id))
                {
                    orderIdsStr = id;
                }

                if (!string.IsNullOrEmpty(orderIdsStr))
                {
                    var orderIds = orderIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                              .Where(g => g != Guid.Empty)
                                              .ToList();

                    foreach (var orderId in orderIds)
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
                                        Amount = (decimal)(session.AmountTotal ?? 0) / 100m, // Note: This might be total for all orders, ideally should be per order
                                        PaymentType = TafsilkPlatform.Models.Models.Enums.PaymentType.Card,
                                        TransactionType = TafsilkPlatform.Models.Models.Enums.TransactionType.Credit,
                                        Provider = "Stripe",
                                        CreatedAt = DateTimeOffset.UtcNow
                                    };
                                    // Adjust amount if multiple orders (simple split or fetch actual order total)
                                    if (orderIds.Count > 1)
                                    {
                                        payment.Amount = (decimal)order.TotalPrice;
                                    }
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
                    }

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
