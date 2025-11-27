using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;
using TafsilkPlatform.Models.ViewModels.Tailor;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for managing customer and tailor profiles, including public views and searches
/// </summary>
[Route("profile")]
[Authorize]
public class ProfilesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProfilesController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IProfileCompletionService _profileCompletionService;
    private readonly IAttachmentService _attachmentService;

    public ProfilesController(
      ApplicationDbContext db,
        ILogger<ProfilesController> logger,
   IFileUploadService fileUploadService,
        IProfileCompletionService profileCompletionService,
        IAttachmentService attachmentService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _profileCompletionService = profileCompletionService ?? throw new ArgumentNullException(nameof(profileCompletionService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
    }

    #region Customer Profile Actions

    /// <summary>
    /// Update customer profile
    /// POST: /profile/customer/edit
    /// </summary>
    [HttpPost("customer/edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> EditCustomerProfile(CustomerProfileEditViewModel model)
    {
        _logger.LogInformation("EditCustomerProfile POST action called for user");

        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User ID retrieved: {UserId}", userId);

            if (userId == Guid.Empty) return Unauthorized();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                // Reload user email for display
                var user = await _db.Users.FindAsync(userId);
                model.Email = user?.Email;

                return View("CustomerProfile", model);
            }

            var customer = await _db.CustomerProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                // Create if not exists (shouldn't happen for existing users, but good fallback)
                customer = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CustomerProfiles.Add(customer);
                _logger.LogInformation("Creating new customer profile for user {UserId}", userId);
            }

            // Update fields
            _logger.LogInformation("Updating customer profile. Old Gender: {OldGender}, New Gender: {NewGender}", customer.Gender, model.Gender);
            customer.FullName = model.FullName;
            customer.City = model.City;
            customer.Gender = string.IsNullOrWhiteSpace(model.Gender) ? null : model.Gender;
            customer.Bio = model.Bio;
            customer.DateOfBirth = model.DateOfBirth;
            customer.UpdatedAt = DateTime.UtcNow;

            // Explicitly mark as modified to ensure EF Core tracks changes
            if (_db.Entry(customer).State == Microsoft.EntityFrameworkCore.EntityState.Unchanged)
            {
                _db.Entry(customer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            // Update user phone number if provided
            if (model.PhoneNumber != null) // Allow clearing phone number if empty string passed? ViewModel has string?
            {
                var user = customer.User ?? await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    _logger.LogInformation("Updating user phone. Old Phone: {OldPhone}, New Phone: {NewPhone}", user.PhoneNumber, model.PhoneNumber);

                    // Check if phone already exists for OTHER users
                    if (!string.IsNullOrEmpty(model.PhoneNumber))
                    {
                        var phoneExists = await _db.Users
                            .AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);

                        if (phoneExists)
                        {
                            ModelState.AddModelError("PhoneNumber", "Phone number is already in use");

                            // Reload user email for display
                            model.Email = user.Email;
                            return View("CustomerProfile", model);
                        }
                    }

                    user.PhoneNumber = model.PhoneNumber;
                    user.UpdatedAt = DateTime.UtcNow;
                    _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                else
                {
                    _logger.LogWarning("User entity not found for customer {CustomerId}", customer.Id);
                }
            }

            var changesCount = await _db.SaveChangesAsync();
            _logger.LogInformation("Customer profile updated for user {UserId}. Changes saved: {ChangesCount}", userId, changesCount);

            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction(nameof(CustomerProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile");
            TempData["Error"] = "An error occurred while saving data";
            return RedirectToAction(nameof(CustomerProfile));
        }
    }

    /// <summary>
    /// View customer profile with addresses and saved tailors
    /// GET: /profile/customer
    /// </summary>
    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CustomerProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var customer = await _db.CustomerProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                _logger.LogInformation("Creating default customer profile for user {UserId}", userId);

                // Create a default profile for this customer
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User {UserId} not found when creating customer profile", userId);
                    return Unauthorized();
                }

                customer = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = user.Email, // Default to email until user updates their profile
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };

                _db.CustomerProfiles.Add(customer);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created default customer profile for user {UserId}", userId);
            }

            // Map to ViewModel
            var model = new CustomerProfileEditViewModel
            {
                FullName = customer.FullName ?? string.Empty,
                PhoneNumber = customer.User?.PhoneNumber,
                City = customer.City,
                Gender = customer.Gender,
                Bio = customer.Bio,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.User?.Email,
                MemberSince = customer.User?.CreatedAt ?? customer.CreatedAt,
                TotalOrders = await _db.Orders.CountAsync(o => o.CustomerId == customer.Id)
            };

            // Get profile completion
            var completion = await _profileCompletionService.GetCustomerCompletionAsync(userId);
            ViewBag.ProfileCompletion = completion;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer profile");
            TempData["Error"] = "An error occurred while loading profile";
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion

    #region Tailor Profile Actions

    /// <summary>
    /// View tailor profile with services and portfolio
    /// GET: /profile/tailor
    /// </summary>
    [HttpGet("tailor")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> TailorProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var tailor = await _db.TailorProfiles
                .Include(t => t.User)
                    .Include(t => t.TailorServices)
                .Include(t => t.PortfolioImages)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor profile not found for user {UserId}", userId);
                return NotFound("Profile not found");
            }

            ViewBag.ServiceCount = tailor.TailorServices.Count(s => !s.IsDeleted);
            ViewBag.PortfolioCount = tailor.PortfolioImages.Count(p => !p.IsDeleted);
            ViewBag.ReviewCount = 0; // Simplified - no reviews

            // Get profile completion
            var completion = await _profileCompletionService.GetTailorCompletionAsync(userId);
            ViewBag.ProfileCompletion = completion;

            return View(tailor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor profile");
            TempData["Error"] = "An error occurred while loading profile";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Edit tailor profile
    /// GET: /profile/tailor/edit
    /// </summary>
    [HttpGet("tailor/edit")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> EditTailorProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var tailor = await _db.TailorProfiles
     .Include(t => t.User)
   .Include(t => t.TailorServices)
          .Include(t => t.PortfolioImages)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
                return NotFound("Profile not found");

            var model = new EditTailorProfileViewModel
            {
                TailorId = tailor.Id,
                UserId = tailor.UserId,
                FullName = tailor.FullName ?? string.Empty,
                PhoneNumber = tailor.User?.PhoneNumber ?? string.Empty,
                Email = tailor.User?.Email,
                ShopName = tailor.ShopName,
                ShopDescription = tailor.ShopDescription,
                Specialization = tailor.Specialization,
                YearsOfExperience = tailor.YearsOfExperience,
                City = tailor.City,
                District = tailor.District,
                Address = tailor.Address,
                Longitude = tailor.Longitude,
                Latitude = tailor.Latitude,
                Bio = tailor.Bio,
                BusinessHours = tailor.BusinessHours,
                FacebookUrl = tailor.FacebookUrl,
                InstagramUrl = tailor.InstagramUrl,
                TwitterUrl = tailor.TwitterUrl,
                WebsiteUrl = tailor.WebsiteUrl,
                CurrentProfilePictureData = tailor.ProfilePictureData,
                CurrentProfilePictureContentType = tailor.ProfilePictureContentType,
                CurrentProfilePictureUrl = tailor.ProfileImageUrl,

                // Statistics
                TotalOrders = await _db.Orders.CountAsync(o => o.TailorId == tailor.Id),
                CompletedOrders = await _db.Orders.CountAsync(o => o.TailorId == tailor.Id && o.Status == OrderStatus.Delivered),
                AverageRating = tailor.AverageRating,
                ReviewCount = 0, // Simplified - no reviews
                PortfolioCount = tailor.PortfolioImages?.Count(p => !p.IsDeleted) ?? 0,
                ServiceCount = tailor.TailorServices?.Count(s => !s.IsDeleted) ?? 0,
                IsVerified = tailor.IsVerified,
                VerifiedAt = tailor.VerifiedAt
            };

            ViewBag.Cities = EgyptCities.GetAll();
            ViewBag.Specializations = TailorSpecializations.GetAll();

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor profile for editing");
            TempData["Error"] = "An error occurred while loading profile";
            return RedirectToAction(nameof(TailorProfile));
        }
    }

    /// <summary>
    /// Update tailor profile
    /// POST: /profile/tailor/edit
    /// </summary>
    [HttpPost("tailor/edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> EditTailorProfile(EditTailorProfileViewModel model)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewBag.Cities = EgyptCities.GetAll();
                ViewBag.Specializations = TailorSpecializations.GetAll();
                return View(model);
            }

            var tailor = await _db.TailorProfiles
      .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
                return NotFound("Profile not found");

            // Ensure user is editing their own profile
            if (tailor.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to edit profile for different user", userId);
                return Unauthorized();
            }

            // Update personal information
            tailor.FullName = model.FullName;

            // Update user phone number if changed
            if (tailor.User != null && !string.IsNullOrEmpty(model.PhoneNumber))
            {
                // Check if phone number is already taken by another user
                var phoneExists = await _db.Users
                          .AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);

                if (phoneExists)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is already in use");
                    ViewBag.Cities = EgyptCities.GetAll();
                    ViewBag.Specializations = TailorSpecializations.GetAll();
                    return View(model);
                }

                tailor.User.PhoneNumber = model.PhoneNumber;
            }

            // Update shop details
            tailor.ShopName = model.ShopName;
            tailor.ShopDescription = model.ShopDescription;
            tailor.Specialization = model.Specialization;
            tailor.YearsOfExperience = model.YearsOfExperience;

            // Update location
            tailor.City = model.City;
            tailor.District = model.District;
            tailor.Address = model.Address;
            tailor.Longitude = model.Longitude;
            tailor.Latitude = model.Latitude;

            // Update bio and business info
            tailor.Bio = model.Bio;
            tailor.BusinessHours = model.BusinessHours;

            // Update social media links
            tailor.FacebookUrl = model.FacebookUrl;
            tailor.InstagramUrl = model.InstagramUrl;
            tailor.TwitterUrl = model.TwitterUrl;
            tailor.WebsiteUrl = model.WebsiteUrl;

            // Handle profile picture upload - prefer filesystem (Attachments/profile)
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Validate image
                if (!await _fileUploadService.IsValidImageAsync(model.ProfilePicture))
                {
                    ModelState.AddModelError(nameof(model.ProfilePicture), "Invalid file type. Please select an image");
                    ViewBag.Cities = EgyptCities.GetAll();
                    ViewBag.Specializations = TailorSpecializations.GetAll();
                    return View(model);
                }

                if (model.ProfilePicture.Length > _fileUploadService.GetMaxFileSizeInBytes())
                {
                    ModelState.AddModelError(nameof(model.ProfilePicture), "File size is too large");
                    return View(model);
                }

                try
                {
                    var uploadedRelative = await _attachmentService.Upload(model.ProfilePicture, "profile");
                    if (!string.IsNullOrEmpty(uploadedRelative))
                    {
                        tailor.ProfileImageUrl = uploadedRelative.StartsWith('/') ? uploadedRelative : "/" + uploadedRelative;
                        // clear DB blob to prefer filesystem storage
                        tailor.ProfilePictureData = null;
                        tailor.ProfilePictureContentType = null;
                    }
                    else
                    {
                        // Fallback to DB storage if attachment service rejects
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.ProfilePicture.CopyToAsync(memoryStream);
                            tailor.ProfilePictureData = memoryStream.ToArray();
                            tailor.ProfilePictureContentType = model.ProfilePicture.ContentType;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading profile picture via AttachmentService, falling back to DB storage");
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfilePicture.CopyToAsync(memoryStream);
                        tailor.ProfilePictureData = memoryStream.ToArray();
                        tailor.ProfilePictureContentType = model.ProfilePicture.ContentType;
                    }
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Tailor profile updated for user {UserId}", userId);
            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction(nameof(TailorProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tailor profile");
            ModelState.AddModelError("", "An error occurred while updating profile");
            ViewBag.Cities = EgyptCities.GetAll();
            ViewBag.Specializations = TailorSpecializations.GetAll();
            return View(model);
        }
    }

    /// <summary>
    /// Get tailor profile picture
    /// GET: /profile/tailor/picture/{id}
    /// </summary>
    [HttpGet("tailor/picture/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTailorProfilePicture(Guid id)
    {
        try
        {
            var tailor = await _db.TailorProfiles
     .FirstOrDefaultAsync(t => t.Id == id);

            if (tailor == null)
            {
                return NotFound();
            }

            // Prefer filesystem-backed image
            if (!string.IsNullOrWhiteSpace(tailor.ProfileImageUrl))
            {
                var imgUrl = tailor.ProfileImageUrl;

                if (Uri.IsWellFormedUriString(imgUrl, UriKind.Absolute))
                {
                    return Redirect(imgUrl);
                }

                if (imgUrl.StartsWith('/'))
                {
                    var relativePath = imgUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        var contentType = tailor.ProfilePictureContentType ?? "image/jpeg";
                        return PhysicalFile(physicalPath, contentType);
                    }
                }

                // If not physical, redirect
                return Redirect(imgUrl);
            }

            // Fallback to DB stored image
            if (tailor.ProfilePictureData == null)
            {
                return NotFound();
            }

            return File(tailor.ProfilePictureData, tailor.ProfilePictureContentType ?? "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile picture for tailor {TailorId}", id);
            return NotFound();
        }
    }

    #endregion


    /* REMOVED: Corporate Profile Actions - Corporate feature has been removed

    // All Corporate profile methods commented out as Corporate feature was removed
    // If you need to restore this functionality, uncomment this section and:
    // 1. Restore CorporateAccount model
    // 2. Restore ICorporateRepository  
    // 3. Restore Corporate views
   // 4. Add Corporate registration role back

    */

    #region Helper Methods

    /// <summary>
    /// Get current authenticated user's ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        _logger.LogWarning("Unable to extract valid user ID from claims");
        return Guid.Empty;
    }

    #endregion
}
