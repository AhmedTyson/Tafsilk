using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TafsilkPlatform.Web.Interfaces;

namespace TafsilkPlatform.Web.Controllers
{
 [ApiController]
 [Route("api/admin")]
 [Authorize(Policy = "AdminApiPolicy")]
 [AutoValidateAntiforgeryToken]
 public class AdminController : ControllerBase
 {
 private readonly IUnitOfWork _uow;
 private readonly ILogger<AdminController> _logger;

 public AdminController(IUnitOfWork uow, ILogger<AdminController> logger)
 {
 _uow = uow;
 _logger = logger;
 }

 // GET: api/admin/users
 [HttpGet("users")]
 public async Task<IActionResult> GetUsers()
 {
 try
 {
 var users = await _uow.Context.Users
 .Include(u => u.Role)
 .Include(u => u.CustomerProfile)
 .Include(u => u.TailorProfile)
 .Include(u => u.CorporateAccount)
 .Select(u => new
 {
 u.Id,
 DisplayName = u.CustomerProfile != null ? u.CustomerProfile.FullName :
 u.TailorProfile != null ? u.TailorProfile.FullName :
 u.CorporateAccount != null ? u.CorporateAccount.CompanyName :
 u.Email,
 u.Email,
 Role = u.Role != null ? u.Role.Name : string.Empty,
 u.IsActive,
 u.CreatedAt
 })
 .OrderByDescending(u => u.CreatedAt)
 .ToListAsync();

 return Ok(users);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to get users");
 return StatusCode(500, new { error = "Failed to fetch users" });
 }
 }

 // POST: api/admin/users/{id}/status
 [HttpPost("users/{id:guid}/status")]
 public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateStatusRequest req)
 {
 try
 {
 await _uow.Users.UpdateUserStatusAsync(id, req.IsActive);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to update user status for {UserId}", id);
 return StatusCode(500, new { error = "Failed to update user status" });
 }
 }

 // POST: api/admin/users/{id}/role
 [HttpPost("users/{id:guid}/role")]
 public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest req)
 {
 try
 {
 var role = await _uow.Context.Roles.FirstOrDefaultAsync(r => r.Name == req.RoleName);
 if (role == null) return BadRequest(new { error = "Role not found" });

 var user = await _uow.Context.Users.FindAsync(id);
 if (user == null) return NotFound(new { error = "User not found" });

 user.RoleId = role.Id;
 _uow.Context.Users.Update(user);
 await _uow.SaveChangesAsync();

 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to change role for user {UserId}", id);
 return StatusCode(500, new { error = "Failed to change user role" });
 }
 }

 // GET: api/admin/tailors/pending
 [HttpGet("tailors/pending")]
 public async Task<IActionResult> GetPendingTailors()
 {
 try
 {
 var tailors = await _uow.Context.TailorProfiles
 .Where(t => !t.IsVerified)
 .Select(t => new
 {
 t.Id,
 DisplayName = t.FullName ?? t.ShopName,
 ExperienceYears = t.ExperienceYears,
 DocumentsCount = _uow.Context.PortfolioImages.Count(pi => pi.TailorId == t.Id),
 CreatedAt = t.CreatedAt,
 t.IsVerified
 })
 .ToListAsync();

 return Ok(tailors);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to get pending tailors");
 return StatusCode(500, new { error = "Failed to fetch pending tailors" });
 }
 }

 // POST: api/admin/tailors/{id}/approve
 [HttpPost("tailors/{id:guid}/approve")]
 public async Task<IActionResult> ApproveTailor(Guid id)
 {
 try
 {
 var tailor = await _uow.Context.TailorProfiles.FindAsync(id);
 if (tailor == null) return NotFound(new { error = "Tailor not found" });
 tailor.IsVerified = true;
 _uow.Context.TailorProfiles.Update(tailor);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to approve tailor {TailorId}", id);
 return StatusCode(500, new { error = "Failed to approve tailor" });
 }
 }

 // POST: api/admin/tailors/{id}/reject
 [HttpPost("tailors/{id:guid}/reject")]
 public async Task<IActionResult> RejectTailor(Guid id)
 {
 try
 {
 var tailor = await _uow.Context.TailorProfiles.FindAsync(id);
 if (tailor == null) return NotFound(new { error = "Tailor not found" });
 tailor.IsVerified = false;
 _uow.Context.TailorProfiles.Update(tailor);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to reject tailor {TailorId}", id);
 return StatusCode(500, new { error = "Failed to reject tailor" });
 }
 }

 // GET: api/admin/reported-content
 [HttpGet("reported-content")]
 public async Task<IActionResult> GetReportedContent()
 {
 try
 {
 var images = await _uow.Context.PortfolioImages
 .OrderByDescending(p => p.UploadedAt)
 .Take(50)
 .Select(p => new { p.PortfolioImageId, p.ImageUrl, p.TailorId, p.UploadedAt })
 .ToListAsync();

 var reviews = await _uow.Context.Reviews
 .Where(r => r.IsDeleted == false)
 .OrderByDescending(r => r.CreatedAt)
 .Take(50)
 .Select(r => new { r.ReviewId, r.Comment, r.Rating, r.TailorId, r.CreatedAt, UserName = r.Customer != null ? r.Customer.FullName : null })
 .ToListAsync();

 return Ok(new { images, reviews });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to get reported content");
 return StatusCode(500, new { error = "Failed to fetch content" });
 }
 }

 // POST: api/admin/content/image/{id}/delete
 [HttpPost("content/image/{id:guid}/delete")]
 public async Task<IActionResult> DeleteImage(Guid id)
 {
 try
 {
 var img = await _uow.Context.PortfolioImages.FindAsync(id);
 if (img == null) return NotFound(new { error = "Image not found" });
 _uow.Context.PortfolioImages.Remove(img);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to delete image {ImageId}", id);
 return StatusCode(500, new { error = "Failed to delete image" });
 }
 }

 // POST: api/admin/reviews/{id}/approve
 [HttpPost("reviews/{id:guid}/approve")]
 public async Task<IActionResult> ApproveReview(Guid id)
 {
 try
 {
 var review = await _uow.Context.Reviews.FindAsync(id);
 if (review == null) return NotFound(new { error = "Review not found" });
 review.IsDeleted = false;
 _uow.Context.Reviews.Update(review);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to approve review {ReviewId}", id);
 return StatusCode(500, new { error = "Failed to approve review" });
 }
 }

 // POST: api/admin/reviews/{id}/reject
 [HttpPost("reviews/{id:guid}/reject")]
 public async Task<IActionResult> RejectReview(Guid id)
 {
 try
 {
 var review = await _uow.Context.Reviews.FindAsync(id);
 if (review == null) return NotFound(new { error = "Review not found" });
 review.IsDeleted = true; // soft-delete
 _uow.Context.Reviews.Update(review);
 await _uow.SaveChangesAsync();
 return Ok(new { success = true });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to reject review {ReviewId}", id);
 return StatusCode(500, new { error = "Failed to reject review" });
 }
 }

 // DTOs used by endpoints
 public record UpdateStatusRequest(bool IsActive);
 public record ChangeRoleRequest(string RoleName);
 }
}
