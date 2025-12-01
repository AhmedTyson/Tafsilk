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
    /// GET: /payments/process?orderId={orderId}
    /// </summary>
    [HttpGet("process")]
    public async Task<IActionResult> ProcessPayment(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Processing payment for OrderId: {OrderId}", orderId);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("User not authenticated for OrderId: {OrderId}", orderId);
                return Unauthorized();
            }

            var order = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Tailor)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                _logger.LogError("Order NOT FOUND in database: {OrderId}", orderId);
                return NotFound("Order not found");
            }

            // Authorization check
            var customer = await _db.CustomerProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                _logger.LogError("Customer profile not found for UserId: {UserId}", userId);
                return Forbid();
            }

            if (order.CustomerId != customer.Id)
            {
                _logger.LogWarning("Authorization failed. OrderCustomer: {OrderCustomer}, CurrentCustomer: {CurrentCustomer}", order.CustomerId, customer.Id);
                return Forbid();
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Cannot pay for cancelled order";
                return RedirectToAction("OrderDetails", "Orders", new { id = orderId });
            }

            // Check if already paid
            var isPaid = await _db.Payment.AnyAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);
            if (isPaid)
            {
                TempData["InfoMessage"] = "This order is already paid. Redirecting you to the store.";
                return RedirectToAction("Index", "Store", new { area = "Customer" });
            }

            // Construct description from product names
            var productNames = order.Items
                .Select(i => i.Product?.Name ?? i.Description)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            var description = productNames.Any()
                ? string.Join(", ", productNames)
                : $"Order #{order.OrderId.ToString().Substring(0, 8).ToUpper()}";

            // Create Checkout Session
            var checkoutRequest = new CreateCheckoutSessionRequest
            {
                OrderId = order.OrderId,
                Amount = (decimal)order.TotalPrice,
                Currency = "EGP",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/payments/success?orderId={order.OrderId}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/payments/cancelled?orderId={order.OrderId}",
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
            return RedirectToAction("OrderDetails", "Orders", new { id = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment page for order {OrderId}", orderId);
            return RedirectToAction("OrderDetails", "Orders", new { id = orderId });
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
    /// GET: /payments/success?orderId={orderId}&session_id={session_id}
    /// </summary>
    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess(Guid orderId, [FromQuery(Name = "session_id")] string? sessionId)
    {
        var model = new TafsilkPlatform.Models.ViewModels.Payments.PaymentSuccessViewModel
        {
            OrderId = orderId,
            IsSuccess = false,
            Message = "Payment verification failed or pending.",
            PaymentStatus = "Pending"
        };

        // Fetch order details to populate the view model
        var order = await _db.Orders
            .Include(o => o.Tailor)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order != null)
        {
            model.OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(); // Or use a dedicated OrderNumber property if available
            model.OrderDate = order.CreatedAt;
            model.PaymentMethod = "Credit/Debit Card"; // Default for this controller
            model.Amount = (decimal)order.TotalPrice; // Ensure amount is set from order if not verified yet
            model.TailorName = order.Tailor?.FullName;
            model.TailorShopName = order.Tailor?.ShopName;
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            try
            {
                // Verify payment status directly from Stripe (in case webhook is delayed)
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
                _logger.LogError(ex, "Error verifying checkout session {SessionId} for order {OrderId}", sessionId, orderId);
            }
        }
        else
        {
            // Fallback: Check database if session_id is missing (e.g. direct navigation)
            var payment = await _db.Payment.FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == TafsilkPlatform.Models.Models.Enums.PaymentStatus.Completed);
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
    /// Restores cart items and deletes the pending order
    /// </summary>
    [HttpGet("cancelled")]
    public async Task<IActionResult> PaymentCancelled(Guid orderId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var success = await _storeService.CancelPendingOrderAsync(orderId, userId);

        if (success)
        {
            TempData["InfoMessage"] = "Payment cancelled. You can choose a different payment method.";
        }
        else
        {
            TempData["ErrorMessage"] = "Payment cancelled, but we couldn't update the order status.";
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


