using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.Tailor;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for managing customer and tailor profiles, including public views and searches
/// </summary>
[Route("profile")]
[Authorize]
public class ProfilesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProfilesController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IProfileCompletionService _profileCompletionService;

    public ProfilesController(
      AppDbContext db,
        ILogger<ProfilesController> logger,
   IFileUploadService fileUploadService,
        IProfileCompletionService profileCompletionService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _profileCompletionService = profileCompletionService ?? throw new ArgumentNullException(nameof(profileCompletionService));
    }

    #region Customer Profile Actions

    /// <summary>
    /// Complete customer profile form
    /// GET: /profile/complete-customer
    /// </summary>
    [HttpGet("complete-customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CompleteCustomerProfile()
    {
      try
        {
 var userId = GetCurrentUserId();
   if (userId == Guid.Empty) return Unauthorized();

      // Check if profile already completed
   var hasProfile = await _db.CustomerProfiles.AnyAsync(c => c.UserId == userId);
            if (hasProfile)
      {
      TempData["Info"] = "تم إكمال ملفك الشخصي مسبقاً";
return RedirectToAction(nameof(CustomerProfile));
     }

        var model = new TafsilkPlatform.Web.ViewModels.CompleteCustomerProfileRequest
         {
    // Pre-fill from user data if available
       };

       return View(model);
      }
      catch (Exception ex)
    {
 _logger.LogError(ex, "Error loading customer profile completion form");
      TempData["Error"] = "حدث خطأ أثناء تحميل النموذج";
         return RedirectToAction("Index", "Home");
  }
    }

    /// <summary>
    /// Save completed customer profile
    /// POST: /profile/complete-customer
    /// </summary>
    [HttpPost("complete-customer")]
    [ValidateAntiForgeryToken]
 [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CompleteCustomerProfile(TafsilkPlatform.Web.ViewModels.CompleteCustomerProfileRequest model)
    {
 try
 {
  var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

       if (!ModelState.IsValid)
    return View(model);

            // Check if profile already exists
     var existingProfile = await _db.CustomerProfiles
     .FirstOrDefaultAsync(c => c.UserId == userId);

if (existingProfile != null)
  {
       TempData["Info"] = "تم إكمال الملف الشخصي مسبقاً";
   return RedirectToAction(nameof(CustomerProfile));
 }

  // Create customer profile
    var profile = new CustomerProfile
       {
       Id = Guid.NewGuid(),
  UserId = userId,
   FullName = model.FullName,
           City = model.City,
            Gender = model.Gender,
      Bio = model.Bio,
       CreatedAt = DateTime.UtcNow
      };

        // Update user phone number if provided
         var user = await _db.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(model.PhoneNumber))
{
       // Check if phone already exists
       var phoneExists = await _db.Users
     .AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);

     if (phoneExists)
       {
     ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل");
     return View(model);
     }

   user.PhoneNumber = model.PhoneNumber;
  }

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
    {
   if (_fileUploadService.IsValidImage(model.ProfilePicture))
      {
              if (model.ProfilePicture.Length <= _fileUploadService.GetMaxFileSizeInBytes())
        {
    using (var memoryStream = new MemoryStream())
          {
        await model.ProfilePicture.CopyToAsync(memoryStream);
           profile.ProfilePictureData = memoryStream.ToArray();
   profile.ProfilePictureContentType = model.ProfilePicture.ContentType;
     }
}
         else
    {
 ModelState.AddModelError(nameof(model.ProfilePicture), "حجم الملف كبير جداً");
    return View(model);
    }
    }
     else
         {
    ModelState.AddModelError(nameof(model.ProfilePicture), "نوع الملف غير صالح");
         return View(model);
      }
   }

  _db.CustomerProfiles.Add(profile);
       await _db.SaveChangesAsync();

    _logger.LogInformation("Customer profile completed for user {UserId}", userId);
      TempData["Success"] = "تم إكمال ملفك الشخصي بنجاح! يمكنك الآن تصفح الخياطين وطلب الخدمات";

     return RedirectToAction("Index", "Tailors");
   }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing customer profile");
  ModelState.AddModelError("", "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى");
     return View(model);
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
                _logger.LogWarning("Customer profile not found for user {UserId}", userId);
                return NotFound("الملف الشخصي غير موجود");
            }

            // Get addresses
            var addresses = await _db.UserAddresses
                  .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                 .ToListAsync();

            ViewBag.Addresses = addresses;
            ViewBag.AddressCount = addresses.Count;

            // Get profile completion
            var completion = await _profileCompletionService.GetCustomerCompletionAsync(userId);
            ViewBag.ProfileCompletion = completion;

            return View(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer profile");
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion

    #region Address Management

    /// <summary>
    /// Manage delivery addresses
    /// GET: /profile/addresses
    /// </summary>
    [HttpGet("addresses")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ManageAddresses()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var addresses = await _db.UserAddresses
             .Where(a => a.UserId == userId)
                          .OrderByDescending(a => a.IsDefault)
                 .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

            return View(addresses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading addresses");
            TempData["Error"] = "حدث خطأ أثناء تحميل العناوين";
            return RedirectToAction(nameof(CustomerProfile));
        }
    }

    /// <summary>
    /// Add new address form
    /// GET: /profile/addresses/add
    /// </summary>
    [HttpGet("addresses/add")]
    [Authorize(Roles = "Customer")]
    public IActionResult AddAddress()
    {
        return View(new UserAddress { UserId = GetCurrentUserId() });
    }

    /// <summary>
    /// Save new address
    /// POST: /profile/addresses/add
    /// </summary>
    [HttpPost("addresses/add")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddAddress(UserAddress model)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            // Ensure user can only add their own addresses
            if (model.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to add address for different user", userId);
                return Unauthorized();
            }

            if (!ModelState.IsValid)
                return View(model);

            // If this is the first address or marked as default, unset other defaults
            if (model.IsDefault || !await _db.UserAddresses.AnyAsync(a => a.UserId == userId))
            {
                var existingDefaults = await _db.UserAddresses
           .Where(a => a.UserId == userId && a.IsDefault)
               .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            _db.UserAddresses.Add(model);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Address added for user {UserId}", userId);
            TempData["Success"] = "تم إضافة العنوان بنجاح";

            return RedirectToAction(nameof(ManageAddresses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address");
            ModelState.AddModelError("", "حدث خطأ أثناء إضافة العنوان");
            return View(model);
        }
    }

    /// <summary>
    /// Edit address form
    /// GET: /profile/addresses/edit/{id}
    /// </summary>
    [HttpGet("addresses/edit/{id:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> EditAddress(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var address = await _db.UserAddresses
       .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound("العنوان غير موجود");

            return View(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading address {AddressId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل العنوان";
            return RedirectToAction(nameof(ManageAddresses));
        }
    }

    /// <summary>
    /// Update address
    /// POST: /profile/addresses/edit/{id}
    /// </summary>
    [HttpPost("addresses/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> EditAddress(Guid id, UserAddress model)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            if (id != model.Id)
                return BadRequest();

            var address = await _db.UserAddresses
          .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound("العنوان غير موجود");

            if (!ModelState.IsValid)
                return View(model);

            // If marked as default, unset other defaults
            if (model.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _db.UserAddresses
              .Where(a => a.UserId == userId && a.IsDefault && a.Id != id)
                 .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            address.Label = model.Label;
            address.Street = model.Street;
            address.City = model.City;
            address.Latitude = model.Latitude;
            address.Longitude = model.Longitude;
            address.IsDefault = model.IsDefault;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Address {AddressId} updated for user {UserId}", id, userId);
            TempData["Success"] = "تم تحديث العنوان بنجاح";

            return RedirectToAction(nameof(ManageAddresses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", id);
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث العنوان");
            return View(model);
        }
    }

    /// <summary>
    /// Delete address
    /// POST: /profile/addresses/delete/{id}
    /// </summary>
    [HttpPost("addresses/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var address = await _db.UserAddresses
           .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound("العنوان غير موجود");

            var wasDefault = address.IsDefault;

            _db.UserAddresses.Remove(address);
            await _db.SaveChangesAsync();

            // If deleted address was default, make another address default
            if (wasDefault)
            {
                var nextAddress = await _db.UserAddresses
                    .Where(a => a.UserId == userId)
                  .OrderByDescending(a => a.CreatedAt)
              .FirstOrDefaultAsync();

                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    await _db.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Address {AddressId} deleted for user {UserId}", id, userId);
            TempData["Success"] = "تم حذف العنوان بنجاح";

            return RedirectToAction(nameof(ManageAddresses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", id);
            TempData["Error"] = "حدث خطأ أثناء حذف العنوان";
            return RedirectToAction(nameof(ManageAddresses));
        }
    }

    /// <summary>
    /// Set address as default
    /// POST: /profile/addresses/set-default/{id}
    /// </summary>
    [HttpPost("addresses/set-default/{id:guid}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SetDefaultAddress(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var address = await _db.UserAddresses
               .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound();

            // Unset all other defaults
            var existingDefaults = await _db.UserAddresses
                       .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }

            address.IsDefault = true;
            await _db.SaveChangesAsync();

            TempData["Success"] = "تم تعيين العنوان الافتراضي بنجاح";
            return RedirectToAction(nameof(ManageAddresses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId}", id);
            TempData["Error"] = "حدث خطأ أثناء تعيين العنوان الافتراضي";
            return RedirectToAction(nameof(ManageAddresses));
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
                .Include(t => t.Reviews)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor profile not found for user {UserId}", userId);
                return NotFound("الملف الشخصي غير موجود");
            }

            ViewBag.ServiceCount = tailor.TailorServices.Count(s => !s.IsDeleted);
            ViewBag.PortfolioCount = tailor.PortfolioImages.Count(p => !p.IsDeleted);
            ViewBag.ReviewCount = tailor.Reviews.Count();

            // Get profile completion
            var completion = await _profileCompletionService.GetTailorCompletionAsync(userId);
            ViewBag.ProfileCompletion = completion;

            return View(tailor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor profile");
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
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
.Include(t => t.Reviews)
   .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
                return NotFound("الملف الشخصي غير موجود");

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

                // Statistics
                TotalOrders = await _db.Orders.CountAsync(o => o.TailorId == tailor.Id),
                CompletedOrders = await _db.Orders.CountAsync(o => o.TailorId == tailor.Id && o.Status == OrderStatus.Delivered),
                AverageRating = tailor.AverageRating,
                ReviewCount = tailor.Reviews?.Count ?? 0,
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
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
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
                return NotFound("الملف الشخصي غير موجود");

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
                    ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل");
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

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Validate image
                if (!_fileUploadService.IsValidImage(model.ProfilePicture))
                {
                    ModelState.AddModelError(nameof(model.ProfilePicture), "نوع الملف غير صالح. يرجى اختيار صورة");
                    ViewBag.Cities = EgyptCities.GetAll();
                    ViewBag.Specializations = TailorSpecializations.GetAll();
                    return View(model);
                }

                if (model.ProfilePicture.Length > _fileUploadService.GetMaxFileSizeInBytes())
                {
                    ModelState.AddModelError(nameof(model.ProfilePicture), "حجم الملف كبير جداً");
                    return View(model);
                }

                // Read and store image data
                using (var memoryStream = new MemoryStream())
                {
                    await model.ProfilePicture.CopyToAsync(memoryStream);
                    tailor.ProfilePictureData = memoryStream.ToArray();
                    tailor.ProfilePictureContentType = model.ProfilePicture.ContentType;
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Tailor profile updated for user {UserId}", userId);
            TempData["Success"] = "تم تحديث الملف الشخصي بنجاح";

            return RedirectToAction(nameof(TailorProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tailor profile");
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث الملف الشخصي");
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

            if (tailor == null || tailor.ProfilePictureData == null)
            {
                // Return default avatar
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
    #region Corporate Profile Actions

    // All Corporate profile methods commented out as Corporate feature was removed
    // If you need to restore this functionality, uncomment this section and:
    // 1. Restore CorporateAccount model
    // 2. Restore ICorporateRepository  
    // 3. Restore Corporate views
   // 4. Add Corporate registration role back

    #endregion
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
