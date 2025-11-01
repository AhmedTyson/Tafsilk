using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Controllers
{
 [Authorize(Roles = "Admin")]
 public class AdminDisputesController : Controller
 {
 private readonly IUnitOfWork _uow;

 public AdminDisputesController(IUnitOfWork uow)
 {
 _uow = uow;
 }

 // Redirect into the single admin dashboard view and instruct client to open disputes list
 public async Task<IActionResult> Index()
 {
 TempData["DisputeAction"] = "index";
 return View("~/Views/Dashboards/admindashboard.cshtml");
 }

 // Open dispute details inside dashboard (client will fetch JSON)
 public async Task<IActionResult> Details(Guid id)
 {
 if (id == Guid.Empty) return BadRequest();
 TempData["DisputeAction"] = "details";
 TempData["DisputeId"] = id.ToString();
 return View("~/Views/Dashboards/admindashboard.cshtml");
 }

 // Show resolve form inside dashboard
 public async Task<IActionResult> Resolve(Guid id)
 {
 if (id == Guid.Empty) return BadRequest();
 TempData["DisputeAction"] = "resolve";
 TempData["DisputeId"] = id.ToString();
 return View("~/Views/Dashboards/admindashboard.cshtml");
 }

 // POST: Resolve dispute (handles form POST and AJAX)
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> ResolveConfirmed(Guid id, string resolutionDetails)
 {
 if (id == Guid.Empty) return BadRequest();

 var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!Guid.TryParse(adminIdClaim, out var adminId))
 {
 return Forbid();
 }

 var success = await _uow.Disputes.ResolveDisputeAsync(id, adminId, resolutionDetails ?? string.Empty);
 if (!success)
 {
 if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
 return Json(new { success = false, message = "تعذر حل النزاع." });

 TempData["Error"] = "تعذر حل النزاع. حاول مرة أخرى.";
 return RedirectToAction(nameof(Index));
 }

 await _uow.SaveChangesAsync();

 if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
 {
 return Json(new { success = true, message = "تم حل النزاع بنجاح." });
 }

 TempData["Success"] = "تم حل النزاع بنجاح.";
 return RedirectToAction(nameof(Index));
 }

 // API: get dispute with order details as JSON
 [HttpGet("AdminDisputes/Get/{id}")]
 public async Task<IActionResult> Get(Guid id)
 {
 if (id == Guid.Empty) return BadRequest();
 var dispute = await _uow.Disputes.GetDisputeWithOrderAsync(id);
 if (dispute is null) return NotFound();
 // return a safe anonymous object to avoid deep graph serialization
 return Json(new
 {
 dispute.Id,
 dispute.OrderId,
 OrderTotal = dispute.Order?.TotalPrice,
 dispute.OpenedByUserId,
 dispute.Reason,
 dispute.Description,
 dispute.Status,
 dispute.ResolutionDetails,
 dispute.ResolvedAt,
 CreatedAt = dispute.CreatedAt.ToString("u")
 });
 }

 // API: get open disputes summary
 [HttpGet("AdminDisputes/GetOpen")]
 public async Task<IActionResult> GetOpen()
 {
 var disputes = await _uow.Disputes.GetOpenDisputesAsync();
 var list = disputes.Select(d => new
 {
 d.Id,
 d.OrderId,
 OrderTotal = d.Order?.TotalPrice,
 d.OpenedByUserId,
 d.Reason,
 d.Status,
 CreatedAt = d.CreatedAt.ToString("u")
 });
 return Json(list);
 }

 // Optional: quick API to get counts by status
 [HttpGet("AdminDisputes/Count/{status}")]
 public async Task<IActionResult> CountByStatus(string status)
 {
 var count = await _uow.Disputes.GetDisputeCountByStatusAsync(status);
 return Ok(new { status, count });
 }
 }
}
