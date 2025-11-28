using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
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

    public PaymentsController(
        IPaymentProcessorService paymentService,
        ApplicationDbContext db,
        ILogger<PaymentsController> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
                TempData["Success"] = "Order is already paid";
                return RedirectToAction("OrderDetails", "Orders", new { id = orderId });
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
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Store/Checkout",
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
            var result = await _paymentService.ConfirmStripePaymentAsync(json, signature);
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
        if (!string.IsNullOrEmpty(sessionId))
        {
            try
            {
                // Verify payment status directly from Stripe (in case webhook is delayed)
                await _paymentService.VerifyCheckoutSessionAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying checkout session {SessionId} for order {OrderId}", sessionId, orderId);
            }
        }

        return View(orderId);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}
