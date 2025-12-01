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
            query = query.Where(u => u.BannedAt == null || (u.BanExpiresAt != null && u.BanExpiresAt <= now));
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
    public async Task<IActionResult> Orders()
    {
        var orders = await _db.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Tailor).ThenInclude(t => t.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var products = await _db.Products
            .Include(p => p.Tailor)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
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
}
