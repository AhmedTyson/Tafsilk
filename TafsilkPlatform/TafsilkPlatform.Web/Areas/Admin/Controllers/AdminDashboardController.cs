using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Admin;
using TafsilkPlatform.Web.Controllers.Base;

namespace TafsilkPlatform.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : BaseController
{
    private readonly ApplicationDbContext _db;

    public AdminDashboardController(
        ApplicationDbContext db,
        ILogger<AdminDashboardController> logger) : base(logger)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Gather key metrics
            var totalUsers = await _db.Users.CountAsync(u => !u.IsDeleted);
            var activeOrders = await _db.Orders.CountAsync(o =>
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled);

            var totalSales = await _db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var totalRevenue = await _db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.CommissionAmount) ?? 0;

            // Get recent activity (Recent Orders for now)
            var recentOrders = await _db.Orders
                .Include(o => o.Customer).ThenInclude(c => c.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new ActivityLogDto
                {
                    Id = o.OrderId,
                    ActivityType = "New Order",
                    Description = $"Order #{o.OrderId.ToString().Substring(0, 8)} placed by {o.Customer.FullName}",
                    Timestamp = o.CreatedAt.DateTime,
                    UserEmail = o.Customer.User.Email
                })
                .ToListAsync();

            // Chart Data: Sales Last 7 Days
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var salesData = await _db.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= sevenDaysAgo)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalPrice) })
                .ToListAsync();

            // Chart Data: Order Status Distribution
            var statusData = await _db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var viewModel = new DashboardHomeViewModel
            {
                TotalUsers = totalUsers,
                ActiveOrders = activeOrders,
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                TotalCommission = totalRevenue,
                TotalNetPayout = totalSales - totalRevenue,
                RecentActivity = recentOrders,
                // Pass chart data via ViewBag or extend ViewModel (using ViewBag for simplicity now)
            };

            ViewBag.SalesDates = salesData.Select(s => s.Date.ToString("MMM dd")).ToArray();
            ViewBag.SalesValues = salesData.Select(s => s.Total).ToArray();
            ViewBag.StatusLabels = statusData.Select(s => s.Status.ToString()).ToArray();
            ViewBag.StatusValues = statusData.Select(s => s.Count).ToArray();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            return View(new DashboardHomeViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Users(string status = "all", string search = "")
    {
        var query = _db.Users
            .Include(u => u.Role)
            .Include(u => u.CustomerProfile)
            .Include(u => u.TailorProfile)
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(search) ||
                (u.CustomerProfile != null && u.CustomerProfile.FullName.ToLower().Contains(search)) ||
                (u.TailorProfile != null && u.TailorProfile.ShopName.ToLower().Contains(search))
            );
        }

        // Filter
        var now = DateTime.UtcNow;
        if (status == "active")
        {
            // Active means: Not banned AND Active AND EmailVerified
            query = query.Where(u =>
                (u.BannedAt == null || (u.BanExpiresAt != null && u.BanExpiresAt <= now)) &&
                u.IsActive &&
                u.EmailVerified
            );
        }
        else if (status == "banned")
        {
            query = query.Where(u => u.BannedAt != null && (u.BanExpiresAt == null || u.BanExpiresAt > now));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(Guid userId, string reason, int? days)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.BannedAt = DateTime.UtcNow;
        user.BanReason = reason;
        user.BanExpiresAt = days.HasValue ? DateTime.UtcNow.AddDays(days.Value) : null;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.BannedAt = null;
        user.BanReason = null;
        user.BanExpiresAt = null;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.IsDeleted = true;
        // Optionally anonymize data or handle related entities

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Orders(string search = "")
    {
        var query = _db.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Tailor).ThenInclude(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
                o.OrderId.ToString().Contains(search) ||
                (o.Customer != null && o.Customer.FullName.ToLower().Contains(search)) ||
                (o.Tailor != null && o.Tailor.ShopName.ToLower().Contains(search))
            );
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentSearch = search;
        return View(orders);
    }

    [HttpGet]

    public async Task<IActionResult> Products(string search = "")
    {
        var query = _db.Products
            .Include(p => p.Tailor)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Category.ToLower().Contains(search) ||
                (p.Tailor != null && p.Tailor.ShopName.ToLower().Contains(search))
            );
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentSearch = search;
        return View(products);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        product.IsDeleted = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(Guid id)
    {
        var order = await _db.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Tailor).ThenInclude(t => t.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(OrderDetails), new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> TailorVerification(string search = "")
    {
        var query = _db.TailorProfiles
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.FullName.ToLower().Contains(search) ||
                t.ShopName.ToLower().Contains(search)
            );
        }

        var tailors = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentSearch = search;
        return View(tailors);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTailor(Guid tailorId)
    {
        var tailor = await _db.TailorProfiles.FindAsync(tailorId);
        if (tailor == null) return NotFound();

        tailor.Verify(DateTime.UtcNow);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tailor verified successfully!";
        return RedirectToAction(nameof(TailorVerification));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTailor(Guid tailorId)
    {
        var tailor = await _db.TailorProfiles.FindAsync(tailorId);
        if (tailor == null) return NotFound();

        tailor.IsVerified = false;
        tailor.VerifiedAt = null;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tailor verification rejected!";
        return RedirectToAction(nameof(TailorVerification));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetMetrics()
    {
        // Fetch all orders with their related data
        var allOrders = await _db.Orders
            .Include(o => o.Payments)
            .Include(o => o.Items)
            .Include(o => o.OrderImages)
            .ToListAsync();

        if (allOrders.Any())
        {
            // Explicitly remove related entities to satisfy foreign key constraints
            foreach (var order in allOrders)
            {
                if (order.Payments != null && order.Payments.Any())
                    _db.RemoveRange(order.Payments);

                if (order.Items != null && order.Items.Any())
                    _db.RemoveRange(order.Items);

                if (order.OrderImages != null && order.OrderImages.Any())
                    _db.RemoveRange(order.OrderImages);
            }

            _db.Orders.RemoveRange(allOrders);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dashboard metrics have been reset. All order history has been cleared.";
        }
        else
        {
            TempData["InfoMessage"] = "No data to reset.";
        }

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> ExportIncomeStatement()
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Tailor)
            .Where(o => o.Status == OrderStatus.Delivered)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var builder = new System.Text.StringBuilder();
        // Header
        builder.AppendLine("Date,Order ID,Tailor,Customer,Product,Quantity,Unit Price (EGP),Total Price (EGP),Platform Commission (EGP)");

        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                var lineDate = order.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                var orderId = order.OrderId.ToString().Substring(0, 8);
                var tailorName = order.Tailor?.ShopName?.Replace(",", " ") ?? "N/A";
                var customerName = order.Customer?.FullName?.Replace(",", " ") ?? "N/A";
                var productName = item.Product?.Name?.Replace(",", " ") ?? "Unknown Product";
                var quantity = item.Quantity;
                var unitPrice = item.UnitPrice;
                var itemTotal = unitPrice * quantity;
                // Commission is per order, but we can list it proportionally or just list 0 for items and full amount for order line? 
                // Better: Calculate per item commission (approx 10%)
                var itemCommission = itemTotal * (decimal)0.10;

                builder.AppendLine($"{lineDate},#{orderId},{tailorName},{customerName},{productName},{quantity},{unitPrice:F2},{itemTotal:F2},{itemCommission:F2}");
            }
        }

        var contentType = "text/csv";
        var fileName = $"IncomeStatement_Admin_{DateTime.Now:yyyyMMddHHmm}.csv";

        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), contentType, fileName);
    }
}
