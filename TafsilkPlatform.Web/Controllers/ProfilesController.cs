using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels;

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

    public ProfilesController(
    AppDbContext db,
        ILogger<ProfilesController> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Customer Profile Actions

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
          ViewBag.ReviewCount = tailor.Reviews.Count;

  return View(tailor);
        }
catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor profile");
   TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
          return RedirectToAction("Index", "Home");
 }
    }

    #endregion

    #region Tailor Service Management

    /// <summary>
    /// Manage tailor services
    /// GET: /profile/tailor/services
    /// </summary>
    [HttpGet("tailor/services")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> ManageServices()
    {
        try
    {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var tailor = await _db.TailorProfiles
         .FirstOrDefaultAsync(t => t.UserId == userId);

  if (tailor == null)
        return NotFound("الملف الشخصي غير موجود");

   var services = await _db.TailorServices
    .Where(s => s.TailorId == tailor.Id && !s.IsDeleted)
        .OrderBy(s => s.ServiceName)
     .ToListAsync();

        ViewBag.TailorId = tailor.Id;

      return View(services);
        }
        catch (Exception ex)
        {
_logger.LogError(ex, "Error loading services");
            TempData["Error"] = "حدث خطأ أثناء تحميل الخدمات";
        return RedirectToAction(nameof(TailorProfile));
      }
    }

    /// <summary>
    /// Add new service form
    /// GET: /profile/tailor/services/add
    /// </summary>
    [HttpGet("tailor/services/add")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> AddService()
    {
   var userId = GetCurrentUserId();
        var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

        if (tailor == null)
    return NotFound();

        return View(new TailorService { TailorId = tailor.Id });
    }

    /// <summary>
    /// Save new service
    /// POST: /profile/tailor/services/add
    /// </summary>
    [HttpPost("tailor/services/add")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> AddService(TailorService model)
    {
 try
        {
       var userId = GetCurrentUserId();
            var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

     if (tailor == null)
            return NotFound();

            // Ensure service belongs to current tailor
      if (model.TailorId != tailor.Id)
     return Unauthorized();

 if (!ModelState.IsValid)
          return View(model);

            model.TailorServiceId = Guid.NewGuid();
            model.IsDeleted = false;

  _db.TailorServices.Add(model);
            await _db.SaveChangesAsync();

      _logger.LogInformation("Service added for tailor {TailorId}", tailor.Id);
            TempData["Success"] = "تم إضافة الخدمة بنجاح";

            return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service");
            ModelState.AddModelError("", "حدث خطأ أثناء إضافة الخدمة");
       return View(model);
    }
    }

    /// <summary>
    /// Edit service form
    /// GET: /profile/tailor/services/edit/{id}
    /// </summary>
    [HttpGet("tailor/services/edit/{id:guid}")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> EditService(Guid id)
    {
        try
 {
       var userId = GetCurrentUserId();
     var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
       return NotFound();

         var service = await _db.TailorServices
    .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id && !s.IsDeleted);

            if (service == null)
       return NotFound("الخدمة غير موجودة");

        return View(service);
     }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading service {ServiceId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل الخدمة";
            return RedirectToAction(nameof(ManageServices));
     }
    }

    /// <summary>
    /// Update service
    /// POST: /profile/tailor/services/edit/{id}
    /// </summary>
    [HttpPost("tailor/services/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> EditService(Guid id, TailorService model)
    {
        try
   {
        var userId = GetCurrentUserId();
  var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

    if (tailor == null || id != model.TailorServiceId)
     return NotFound();

     var service = await _db.TailorServices
     .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id && !s.IsDeleted);

 if (service == null)
                return NotFound();

            if (!ModelState.IsValid)
      return View(model);

      service.ServiceName = model.ServiceName;
            service.Description = model.Description;
   service.BasePrice = model.BasePrice;
     service.EstimatedDuration = model.EstimatedDuration;

       await _db.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} updated for tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "تم تحديث الخدمة بنجاح";

      return RedirectToAction(nameof(ManageServices));
        }
    catch (Exception ex)
  {
    _logger.LogError(ex, "Error updating service {ServiceId}", id);
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث الخدمة");
            return View(model);
        }
    }

    /// <summary>
    /// Delete service (soft delete)
    /// POST: /profile/tailor/services/delete/{id}
    /// </summary>
    [HttpPost("tailor/services/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> DeleteService(Guid id)
  {
    try
      {
         var userId = GetCurrentUserId();
      var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

      if (tailor == null)
                return NotFound();

    var service = await _db.TailorServices
       .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id);

     if (service == null)
       return NotFound();

            service.IsDeleted = true;
        await _db.SaveChangesAsync();

          _logger.LogInformation("Service {ServiceId} deleted for tailor {TailorId}", id, tailor.Id);
      TempData["Success"] = "تم حذف الخدمة بنجاح";

  return RedirectToAction(nameof(ManageServices));
}
        catch (Exception ex)
        {
   _logger.LogError(ex, "Error deleting service {ServiceId}", id);
            TempData["Error"] = "حدث خطأ أثناء حذف الخدمة";
         return RedirectToAction(nameof(ManageServices));
        }
    }

    #endregion

    #region Public Tailor Search

    /// <summary>
    /// Search and filter verified tailors
    /// GET: /profile/search-tailors
    /// </summary>
    [HttpGet("search-tailors")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchTailors(
        string? city = null,
        string? serviceType = null,
    int page = 1,
        int pageSize = 12)
    {
        try
     {
   var query = _db.TailorProfiles
           .Include(t => t.TailorServices)
       .Include(t => t.Reviews)
            .Where(t => t.IsVerified);

       // Apply city filter
          if (!string.IsNullOrWhiteSpace(city))
     {
          query = query.Where(t => t.City != null && t.City.Contains(city));
   }

            // Apply service filter
            if (!string.IsNullOrWhiteSpace(serviceType))
     {
           query = query.Where(t => t.TailorServices.Any(s => 
              !s.IsDeleted && s.ServiceName.Contains(serviceType)));
 }

          // Calculate total count for pagination
   var totalCount = await query.CountAsync();
      var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paginated results
        var tailors = await query
             .OrderByDescending(t => t.IsVerified)
           .ThenByDescending(t => t.Reviews.Count)
          .Skip((page - 1) * pageSize)
   .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentCity = city;
   ViewBag.CurrentServiceType = serviceType;
ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
       ViewBag.TotalResults = totalCount;

            return View(tailors);
 }
        catch (Exception ex)
    {
            _logger.LogError(ex, "Error searching tailors");
        TempData["Error"] = "حدث خطأ أثناء البحث عن الخياطين";
            return View(new List<TailorProfile>());
        }
    }

    /// <summary>
    /// View public tailor profile
    /// GET: /profile/tailor/{id}
    /// </summary>
    [HttpGet("tailor/{id:guid}")]
    [AllowAnonymous]
  public async Task<IActionResult> ViewPublicTailorProfile(Guid id)
    {
     try
      {
 var tailor = await _db.TailorProfiles
            .Include(t => t.TailorServices.Where(s => !s.IsDeleted))
     .Include(t => t.PortfolioImages.Where(p => !p.IsDeleted))
       .Include(t => t.Reviews)
      .FirstOrDefaultAsync(t => t.Id == id);

    if (tailor == null || !tailor.IsVerified)
     {
      return NotFound("الملف الشخصي غير موجود");
   }

       return View(tailor);
        }
      catch (Exception ex)
        {
     _logger.LogError(ex, "Error loading public tailor profile {TailorId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
            return RedirectToAction(nameof(SearchTailors));
        }
    }

    #endregion

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
