using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Utility.Extensions;

namespace TafsilkPlatform.Web.Areas.Tailor.Controllers;

[Area("Tailor")]
[Route("tailor/orders")]
[Authorize(Roles = "Tailor")]
public class TailorOrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TailorOrdersController> _logger;

    public TailorOrdersController(ApplicationDbContext context, ILogger<TailorOrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /tailor/orders
    [HttpGet("")]
    public async Task<IActionResult> Index(string status = "All")
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await _context.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
            {
                return RedirectToAction("Tailor", "Dashboards");
            }

            var query = _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.TailorId == tailor.Id);

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Pending")
                {
                    query = query.Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PendingPayment);
                }
                else if (status == "Active")
                {
                    query = query.Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped);
                }
                else if (status == "Completed")
                {
                    query = query.Where(o => o.Status == OrderStatus.Delivered);
                }
                else if (status == "Cancelled")
                {
                    query = query.Where(o => o.Status == OrderStatus.Cancelled);
                }
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            ViewData["CurrentStatus"] = status;
            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor orders");
            TempData["Error"] = "An error occurred while loading orders";
            return RedirectToAction("Tailor", "Dashboards");
        }
    }

    // POST: /tailor/orders/update-status
    [HttpPost("update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid orderId, OrderStatus newStatus)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await _context.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null) return Unauthorized();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.TailorId == tailor.Id);

            if (order == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(Index));
            }

            // Validate transition (basic validation)
            // Prevent going back to Pending from Confirmed, etc. if needed.
            // For now, allow tailors to manage flow.

            var oldStatus = order.Status;
            order.Status = newStatus;
            // order.UpdatedAt = DateTimeOffset.UtcNow; // Property does not exist

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus} by tailor {TailorId}",
                orderId, oldStatus, newStatus, tailor.Id);

            TempData["Success"] = "Order status updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
            TempData["Error"] = "An error occurred while updating status";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /tailor/orders/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await _context.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                // .Include(o => o.ShippingAddress) // Property does not exist
                .FirstOrDefaultAsync(o => o.OrderId == id && o.TailorId == tailor.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order details {OrderId}", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
