using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services.Payment;

namespace TafsilkPlatform.Web.Controllers;

[Route("payments")]
[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentProcessorService _paymentService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStoreService _storeService;

    public PaymentsController(
        IPaymentProcessorService paymentService,
        ApplicationDbContext db,
        ILogger<PaymentsController> logger,
        IConfiguration configuration,
        IStoreService storeService)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
    }

    /// <summary>
    /// Process payment page
    /// GET: /payments/process?orderIds={id1,id2,...}
    /// </summary>
    [HttpGet("process")]
    public async Task<IActionResult> ProcessPayment(string orderIds)
    {
        try
        {
            if (string.IsNullOrEmpty(orderIds)) return BadRequest("No order IDs provided");

            var orderIdList = orderIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                      .Where(g => g != Guid.Empty)
                                      .ToList();

            if (!orderIdList.Any()) return BadRequest("Invalid order IDs");

            _logger.LogInformation("Processing payment for {OrderCount} orders. IDs: {OrderIds}", orderIdList.Count, orderIds);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Authorization check
            var customer = await _db.CustomerProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                return Forbid();
            }

            var orders = new List<Order>();
            decimal totalAmount = 0;
            var descriptionParts = new List<string>();

            foreach (var orderId in orderIdList)
            {
                var order = await _db.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null) return NotFound($"Order {orderId} not found");

                if (order.CustomerId != customer.Id)
                {
                    _logger.LogWarning("Authorization failed for order {OrderId}", orderId);
                    return Forbid();
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    TempData["Error"] = $"Cannot pay for cancelled order {orderId}";
                    return RedirectToAction("OrderDetails", "Orders", new { id = orderId });
                }

                // Check if already paid
                var isPaid = await _db.Payment.AnyAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);
                if (isPaid)
                {
                    TempData["InfoMessage"] = $"Order {orderId} is already paid.";
                    continue; // Skip paid orders or redirect? For now, let's assume we shouldn't be here if paid.
                }

                orders.Add(order);
                totalAmount += (decimal)order.TotalPrice;

                var productNames = order.Items
                    .Select(i => i.Product?.Name ?? i.Description)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                if (productNames.Any())
                    descriptionParts.Add($"{string.Join(", ", productNames)}");
            }

            if (!orders.Any())
            {
                return RedirectToAction("Index", "Store", new { area = "Customer" });
            }

            var description = string.Join("; ", descriptionParts);
            if (description.Length > 500) description = description.Substring(0, 497) + "...";

            // Create Checkout Session
            var checkoutRequest = new CreateCheckoutSessionRequest
            {
                OrderId = orders.First().OrderId, // Legacy
                OrderIds = orderIdList, // New list
                Amount = totalAmount,
                Currency = "EGP",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/payments/success?orderIds={orderIds}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/payments/cancelled?orderIds={orderIds}",
                CustomerEmail = customer.User?.Email,
                Description = description
            };

            var result = await _paymentService.CreateCheckoutSessionAsync(checkoutRequest);
            if (result.IsSuccess)
            {
                return Redirect(result.Value);
            }

            _logger.LogError("Failed to create checkout session: {Error}", result.Error);
            TempData["Error"] = "Failed to initiate payment. Please try again.";
            return RedirectToAction("OrderDetails", "Orders", new { id = orders.First().OrderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment page");
            return RedirectToAction("Index", "Store", new { area = "Customer" });
        }
    }

    /// <summary>
    /// Create Payment Intent API
    /// POST: /payments/create-intent
    /// </summary>
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var result = await _paymentService.CreatePaymentIntentAsync(request);
            if (result.IsSuccess)
            {
                return Ok(new { clientSecret = result.Value });
            }
            return BadRequest(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Stripe Webhook
    /// POST: /payments/webhook
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        try
        {
            var result = await _paymentService.ConfirmStripePaymentAsync(json, signature.ToString());
            if (result.IsSuccess)
            {
                return Ok();
            }
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing failed");
            return BadRequest();
        }
    }

    /// <summary>
    /// Payment Success Page
    /// GET: /payments/success?orderIds={ids}&session_id={session_id}
    /// </summary>
    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess(string orderIds, [FromQuery(Name = "session_id")] string? sessionId)
    {
        if (string.IsNullOrEmpty(orderIds)) return RedirectToAction("Index", "Store", new { area = "Customer" });

        var orderIdList = orderIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                  .Where(g => g != Guid.Empty)
                                  .ToList();

        if (!orderIdList.Any()) return RedirectToAction("Index", "Store", new { area = "Customer" });

        var firstOrderId = orderIdList.First();

        var model = new TafsilkPlatform.Models.ViewModels.Payments.PaymentSuccessViewModel
        {
            OrderId = firstOrderId, // Main order for display
            IsSuccess = false,
            Message = "Payment verification failed or pending.",
            PaymentStatus = "Pending"
        };

        // Fetch first order details for basic info
        var order = await _db.Orders
            .Include(o => o.Tailor)
            .FirstOrDefaultAsync(o => o.OrderId == firstOrderId);

        if (order != null)
        {
            model.OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper();
            model.OrderDate = order.CreatedAt;
            model.PaymentMethod = "Credit/Debit Card";
            model.Amount = (decimal)order.TotalPrice; // This will be updated below
            model.TailorName = order.Tailor?.FullName;
            model.TailorShopName = order.Tailor?.ShopName;
        }

        decimal totalAmount = 0;
        foreach (var id in orderIdList)
        {
            var o = await _db.Orders.Include(x => x.Tailor).FirstOrDefaultAsync(x => x.OrderId == id);
            if (o != null)
            {
                model.Orders.Add(new TafsilkPlatform.Models.ViewModels.Store.OrderSuccessDetailsViewModel
                {
                    OrderNumber = o.OrderId.ToString().Substring(0, 8).ToUpper(),
                    TotalAmount = (decimal)o.TotalPrice,
                    TailorName = o.Tailor?.FullName ?? "Unknown",
                    TailorShopName = o.Tailor?.ShopName
                });
                totalAmount += (decimal)o.TotalPrice;
            }
        }
        if (totalAmount > 0) model.Amount = totalAmount;

        if (!string.IsNullOrEmpty(sessionId))
        {
            try
            {
                // Verify payment status directly from Stripe
                var result = await _paymentService.VerifyCheckoutSessionAsync(sessionId);
                if (result.IsSuccess && result.Value)
                {
                    model.IsSuccess = true;
                    model.Message = "Payment completed successfully!";
                    model.PaymentStatus = "Completed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying checkout session {SessionId}", sessionId);
            }
        }
        else
        {
            // Fallback: Check database
            var payment = await _db.Payment.FirstOrDefaultAsync(p => p.OrderId == firstOrderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);
            if (payment != null)
            {
                model.IsSuccess = true;
                model.Message = "Payment completed successfully!";
                model.PaymentStatus = "Completed";
                model.Amount = payment.Amount;
            }
        }

        return View(model);
    }

    /// <summary>
    /// Handle payment cancellation
    /// Restores cart items and deletes the pending orders
    /// </summary>
    [HttpGet("cancelled")]
    public async Task<IActionResult> PaymentCancelled(string orderIds)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(orderIds)) return RedirectToAction("Index", "Store", new { area = "Customer" });

        var orderIdList = orderIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                  .Where(g => g != Guid.Empty)
                                  .ToList();

        bool allSuccess = true;
        foreach (var orderId in orderIdList)
        {
            var success = await _storeService.CancelPendingOrderAsync(orderId, userId);
            if (!success) allSuccess = false;
        }

        if (allSuccess)
        {
            TempData["InfoMessage"] = "Payment cancelled. You can choose a different payment method.";
        }
        else
        {
            TempData["ErrorMessage"] = "Payment cancelled, but we couldn't update some order statuses.";
        }

        return RedirectToAction("Index", "Store", new { area = "Customer" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}


