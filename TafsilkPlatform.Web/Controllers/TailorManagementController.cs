using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Extensions;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for tailor to manage their services, portfolio, and pricing
/// </summary>
[Route("tailor/manage")]
[Authorize(Roles = "Tailor")]
public class TailorManagementController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<TailorManagementController> _logger;
    private readonly IFileUploadService _fileUploadService;

    public TailorManagementController(
        AppDbContext context,
        ILogger<TailorManagementController> logger,
        IFileUploadService fileUploadService)
    {
        _context = context;
        _logger = logger;
        _fileUploadService = fileUploadService;
    }

    #region Portfolio Management (معرض الأعمال)

    /// <summary>
    /// View and manage portfolio gallery
    /// GET: /tailor/manage/portfolio
    /// </summary>
    [HttpGet("portfolio")]
    public async Task<IActionResult> ManagePortfolio()
    {
   try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

          if (tailor == null)
                return NotFound("الملف الشخصي غير موجود");

     var portfolioImages = await _context.PortfolioImages
       .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
                .OrderByDescending(p => p.IsFeatured)
       .ThenByDescending(p => p.CreatedAt)
      .ToListAsync();

   var model = new ManagePortfolioViewModel
            {
        TailorId = tailor.Id,
       TailorName = tailor.FullName ?? "خياط",
     Images = portfolioImages.Select(p => new PortfolioItemDto
                {
         Id = p.PortfolioImageId,
     Title = p.Title,
         Category = p.Category,
     Description = p.Description,
     EstimatedPrice = p.EstimatedPrice,
     IsFeatured = p.IsFeatured,
       IsBeforeAfter = p.IsBeforeAfter,
           DisplayOrder = p.DisplayOrder,
  UploadedAt = p.UploadedAt,
       HasImageData = p.ImageData != null
                }).ToList(),
            TotalImages = portfolioImages.Count,
          FeaturedCount = portfolioImages.Count(p => p.IsFeatured),
          MaxAllowedImages = 50 // Configure as needed
 };

        ViewData["Title"] = "إدارة معرض الأعمال";
            return View(model);
  }
        catch (Exception ex)
     {
        _logger.LogError(ex, "Error loading portfolio management");
 TempData["Error"] = "حدث خطأ أثناء تحميل معرض الأعمال";
     return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// <summary>
    /// Add new portfolio image form
 /// GET: /tailor/manage/portfolio/add
    /// </summary>
    [HttpGet("portfolio/add")]
    public async Task<IActionResult> AddPortfolioImage()
    {
        var userId = User.GetUserId();
        var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
          return NotFound();

        var model = new AddPortfolioImageViewModel
        {
            TailorId = tailor.Id
        };

    ViewBag.Categories = GetPortfolioCategories();
        return View(model);
  }

    /// <summary>
    /// Save new portfolio image
    /// POST: /tailor/manage/portfolio/add
    /// </summary>
    [HttpPost("portfolio/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPortfolioImage(AddPortfolioImageViewModel model)
    {
        try
    {
      var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

 if (tailor == null || tailor.Id != model.TailorId)
          return Unauthorized();

  if (!ModelState.IsValid)
            {
      ViewBag.Categories = GetPortfolioCategories();
        return View(model);
     }

         // Validate image file
  if (model.ImageFile == null || model.ImageFile.Length == 0)
     {
      ModelState.AddModelError(nameof(model.ImageFile), "يرجى اختيار صورة");
                ViewBag.Categories = GetPortfolioCategories();
          return View(model);
          }

       if (!_fileUploadService.IsValidImage(model.ImageFile))
            {
       ModelState.AddModelError(nameof(model.ImageFile), "نوع الملف غير صالح. يرجى اختيار صورة (JPG, PNG, GIF)");
    ViewBag.Categories = GetPortfolioCategories();
            return View(model);
            }

            if (model.ImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
            {
      ModelState.AddModelError(nameof(model.ImageFile), $"حجم الصورة يجب أن يكون أقل من {_fileUploadService.GetMaxFileSizeInBytes() / 1024 / 1024} ميجابايت");
       ViewBag.Categories = GetPortfolioCategories();
       return View(model);
 }

            // Check image count limit
         var currentImageCount = await _context.PortfolioImages
      .CountAsync(p => p.TailorId == tailor.Id && !p.IsDeleted);

      if (currentImageCount >= 50) // Configure as needed
            {
    TempData["Error"] = "لقد وصلت إلى الحد الأقصى لعدد الصور (50 صورة)";
   return RedirectToAction(nameof(ManagePortfolio));
            }

 // Read image data
            byte[] imageData;
            using (var memoryStream = new MemoryStream())
            {
            await model.ImageFile.CopyToAsync(memoryStream);
          imageData = memoryStream.ToArray();
            }

   // Get next display order
            var maxOrder = await _context.PortfolioImages
     .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
        .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

        // Create portfolio image
    var portfolioImage = new PortfolioImage
          {
           PortfolioImageId = Guid.NewGuid(),
       TailorId = tailor.Id,
  Title = model.Title,
  Category = model.Category,
      Description = model.Description,
      EstimatedPrice = model.EstimatedPrice,
  IsFeatured = model.IsFeatured,
       IsBeforeAfter = model.IsBeforeAfter,
          DisplayOrder = maxOrder + 1,
          ImageData = imageData,
     ContentType = model.ImageFile.ContentType,
          UploadedAt = DateTime.UtcNow,
   CreatedAt = DateTime.UtcNow,
   IsDeleted = false
 };

     _context.PortfolioImages.Add(portfolioImage);
    await _context.SaveChangesAsync();

  _logger.LogInformation("Portfolio image added for tailor {TailorId}", tailor.Id);
            TempData["Success"] = "تم إضافة الصورة بنجاح إلى معرض الأعمال";

            return RedirectToAction(nameof(ManagePortfolio));
        }
        catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding portfolio image");
            ModelState.AddModelError("", "حدث خطأ أثناء إضافة الصورة");
  ViewBag.Categories = GetPortfolioCategories();
  return View(model);
 }
    }

    /// <summary>
    /// Edit portfolio image
    /// GET: /tailor/manage/portfolio/edit/{id}
    /// </summary>
    [HttpGet("portfolio/edit/{id:guid}")]
    public async Task<IActionResult> EditPortfolioImage(Guid id)
    {
        try
        {
     var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

         if (tailor == null)
                return NotFound();

   var image = await _context.PortfolioImages
         .FirstOrDefaultAsync(p => p.PortfolioImageId == id && p.TailorId == tailor.Id && !p.IsDeleted);

 if (image == null)
            return NotFound("الصورة غير موجودة");

     var model = new EditPortfolioImageViewModel
      {
                Id = image.PortfolioImageId,
      TailorId = tailor.Id,
           Title = image.Title,
     Category = image.Category,
   Description = image.Description,
    EstimatedPrice = image.EstimatedPrice,
      IsFeatured = image.IsFeatured,
         IsBeforeAfter = image.IsBeforeAfter,
        DisplayOrder = image.DisplayOrder,
    HasCurrentImage = image.ImageData != null
};

      ViewBag.Categories = GetPortfolioCategories();
          return View(model);
   }
    catch (Exception ex)
        {
_logger.LogError(ex, "Error loading portfolio image for editing");
            TempData["Error"] = "حدث خطأ أثناء تحميل الصورة";
            return RedirectToAction(nameof(ManagePortfolio));
        }
    }

  /// <summary>
    /// Update portfolio image
    /// POST: /tailor/manage/portfolio/edit/{id}
    /// </summary>
    [HttpPost("portfolio/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPortfolioImage(Guid id, EditPortfolioImageViewModel model)
 {
        try
        {
   var userId = User.GetUserId();
var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null || tailor.Id != model.TailorId)
        return Unauthorized();

 if (id != model.Id)
    return BadRequest();

          var image = await _context.PortfolioImages
                .FirstOrDefaultAsync(p => p.PortfolioImageId == id && p.TailorId == tailor.Id && !p.IsDeleted);

 if (image == null)
     return NotFound();

       if (!ModelState.IsValid)
            {
      ViewBag.Categories = GetPortfolioCategories();
       return View(model);
    }

         // Update image info
         image.Title = model.Title;
    image.Category = model.Category;
            image.Description = model.Description;
            image.EstimatedPrice = model.EstimatedPrice;
 image.IsFeatured = model.IsFeatured;
         image.IsBeforeAfter = model.IsBeforeAfter;
      image.DisplayOrder = model.DisplayOrder;

            // Update image data if new file provided
      if (model.NewImageFile != null && model.NewImageFile.Length > 0)
        {
     if (!_fileUploadService.IsValidImage(model.NewImageFile))
        {
     ModelState.AddModelError(nameof(model.NewImageFile), "نوع الملف غير صالح");
         ViewBag.Categories = GetPortfolioCategories();
   return View(model);
       }

       if (model.NewImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
              {
   ModelState.AddModelError(nameof(model.NewImageFile), "حجم الملف كبير جداً");
      ViewBag.Categories = GetPortfolioCategories();
        return View(model);
       }

         using (var memoryStream = new MemoryStream())
          {
      await model.NewImageFile.CopyToAsync(memoryStream);
          image.ImageData = memoryStream.ToArray();
         image.ContentType = model.NewImageFile.ContentType;
       }
        }

 await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio image {ImageId} updated for tailor {TailorId}", id, tailor.Id);
        TempData["Success"] = "تم تحديث الصورة بنجاح";

            return RedirectToAction(nameof(ManagePortfolio));
   }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error updating portfolio image");
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث الصورة");
            ViewBag.Categories = GetPortfolioCategories();
            return View(model);
        }
    }

    /// <summary>
/// Delete portfolio image
    /// POST: /tailor/manage/portfolio/delete/{id}
    /// </summary>
    [HttpPost("portfolio/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeletePortfolioImage(Guid id)
    {
     try
        {
          var userId = User.GetUserId();
          var tailor = await GetTailorProfileAsync(userId);

  if (tailor == null)
    return Unauthorized();

     var image = await _context.PortfolioImages
   .FirstOrDefaultAsync(p => p.PortfolioImageId == id && p.TailorId == tailor.Id && !p.IsDeleted);

    if (image == null)
      return NotFound();

     // Soft delete
            image.IsDeleted = true;
     await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio image {ImageId} deleted for tailor {TailorId}", id, tailor.Id);
   TempData["Success"] = "تم حذف الصورة بنجاح";

    return RedirectToAction(nameof(ManagePortfolio));
        }
    catch (Exception ex)
      {
 _logger.LogError(ex, "Error deleting portfolio image");
  TempData["Error"] = "حدث خطأ أثناء حذف الصورة";
       return RedirectToAction(nameof(ManagePortfolio));
        }
    }

    /// <summary>
    /// Get portfolio image
    /// GET: /tailor/manage/portfolio/image/{id}
 /// </summary>
    [HttpGet("portfolio/image/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPortfolioImage(Guid id)
    {
      try
        {
            var image = await _context.PortfolioImages
      .FirstOrDefaultAsync(p => p.PortfolioImageId == id && !p.IsDeleted);

            if (image == null || image.ImageData == null)
    return NotFound();

            return File(image.ImageData, image.ContentType ?? "image/jpeg");
      }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Error retrieving portfolio image {ImageId}", id);
      return NotFound();
        }
    }

    /// <summary>
    /// Toggle featured status
    /// POST: /tailor/manage/portfolio/toggle-featured/{id}
    /// </summary>
    [HttpPost("portfolio/toggle-featured/{id:guid}")]
    [ValidateAntiForgeryToken]
  public async Task<IActionResult> ToggleFeatured(Guid id)
    {
        try
        {
   var userId = User.GetUserId();
          var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
           return Unauthorized();

       var image = await _context.PortfolioImages
             .FirstOrDefaultAsync(p => p.PortfolioImageId == id && p.TailorId == tailor.Id && !p.IsDeleted);

   if (image == null)
     return NotFound();

   image.IsFeatured = !image.IsFeatured;
   await _context.SaveChangesAsync();

   return Json(new { success = true, isFeatured = image.IsFeatured });
        }
   catch (Exception ex)
{
            _logger.LogError(ex, "Error toggling featured status");
 return Json(new { success = false, message = "حدث خطأ" });
        }
    }

    #endregion

    #region Service Management (الخدمات)

    /// <summary>
    /// View and manage services
    /// GET: /tailor/manage/services
    /// </summary>
 [HttpGet("services")]
    public async Task<IActionResult> ManageServices()
    {
  try
        {
      var userId = User.GetUserId();
         var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
  return NotFound("الملف الشخصي غير موجود");

      var services = await _context.TailorServices
      .Where(s => s.TailorId == tailor.Id && !s.IsDeleted)
       .OrderBy(s => s.ServiceName)
    .ToListAsync();

     var model = new ManageServicesViewModel
   {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "خياط",
            Services = services.Select(s => new ServiceItemDto
           {
        Id = s.TailorServiceId,
                    ServiceName = s.ServiceName,
    Description = s.Description,
      BasePrice = s.BasePrice,
EstimatedDuration = s.EstimatedDuration
         }).ToList(),
         TotalServices = services.Count,
           AveragePrice = services.Any() ? services.Average(s => s.BasePrice) : 0
};

   ViewData["Title"] = "إدارة الخدمات";
        return View(model);
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Error loading services management");
     TempData["Error"] = "حدث خطأ أثناء تحميل الخدمات";
        return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// <summary>
    /// Add new service form
    /// GET: /tailor/manage/services/add
    /// </summary>
    [HttpGet("services/add")]
    public async Task<IActionResult> AddService()
    {
        var userId = User.GetUserId();
        var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
   return NotFound();

        var model = new AddServiceViewModel
        {
        TailorId = tailor.Id
        };

    ViewBag.ServiceTypes = GetServiceTypes();
        return View(model);
    }

    /// <summary>
    /// Save new service
    /// POST: /tailor/manage/services/add
    /// </summary>
    [HttpPost("services/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddService(AddServiceViewModel model)
    {
     try
      {
  var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null || tailor.Id != model.TailorId)
     return Unauthorized();

         if (!ModelState.IsValid)
     {
             ViewBag.ServiceTypes = GetServiceTypes();
return View(model);
         }

    // Check for duplicate service name
            var existingService = await _context.TailorServices
   .AnyAsync(s => s.TailorId == tailor.Id && s.ServiceName == model.ServiceName && !s.IsDeleted);

        if (existingService)
  {
     ModelState.AddModelError(nameof(model.ServiceName), "يوجد خدمة بنفس الاسم بالفعل");
                ViewBag.ServiceTypes = GetServiceTypes();
 return View(model);
 }

        // Create service
   var service = new TailorService
      {
     TailorServiceId = Guid.NewGuid(),
    TailorId = tailor.Id,
              ServiceName = model.ServiceName,
    Description = model.Description,
     BasePrice = model.BasePrice,
      EstimatedDuration = model.EstimatedDuration,
      IsDeleted = false
        };

            _context.TailorServices.Add(service);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service added for tailor {TailorId}", tailor.Id);
  TempData["Success"] = "تم إضافة الخدمة بنجاح";

   return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service");
   ModelState.AddModelError("", "حدث خطأ أثناء إضافة الخدمة");
         ViewBag.ServiceTypes = GetServiceTypes();
            return View(model);
        }
    }

    /// <summary>
/// Edit service
    /// GET: /tailor/manage/services/edit/{id}
    /// </summary>
    [HttpGet("services/edit/{id:guid}")]
    public async Task<IActionResult> EditService(Guid id)
    {
        try
        {
         var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

     if (tailor == null)
    return NotFound();

            var service = await _context.TailorServices
        .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id && !s.IsDeleted);

            if (service == null)
            return NotFound("الخدمة غير موجودة");

            var model = new EditServiceViewModel
   {
     Id = service.TailorServiceId,
           TailorId = tailor.Id,
                ServiceName = service.ServiceName,
          Description = service.Description,
      BasePrice = service.BasePrice,
     EstimatedDuration = service.EstimatedDuration
            };

     ViewBag.ServiceTypes = GetServiceTypes();
   return View(model);
        }
        catch (Exception ex)
 {
        _logger.LogError(ex, "Error loading service for editing");
  TempData["Error"] = "حدث خطأ أثناء تحميل الخدمة";
  return RedirectToAction(nameof(ManageServices));
   }
    }

    /// <summary>
 /// Update service
    /// POST: /tailor/manage/services/edit/{id}
    /// </summary>
    [HttpPost("services/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(Guid id, EditServiceViewModel model)
    {
try
 {
     var userId = User.GetUserId();
   var tailor = await GetTailorProfileAsync(userId);

       if (tailor == null || tailor.Id != model.TailorId)
   return Unauthorized();

            if (id != model.Id)
        return BadRequest();

  var service = await _context.TailorServices
           .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id && !s.IsDeleted);

       if (service == null)
       return NotFound();

        if (!ModelState.IsValid)
 {
                ViewBag.ServiceTypes = GetServiceTypes();
       return View(model);
            }

       // Check for duplicate service name (excluding current service)
            var existingService = await _context.TailorServices
        .AnyAsync(s => s.TailorId == tailor.Id && s.ServiceName == model.ServiceName && s.TailorServiceId != id && !s.IsDeleted);

            if (existingService)
    {
    ModelState.AddModelError(nameof(model.ServiceName), "يوجد خدمة بنفس الاسم بالفعل");
   ViewBag.ServiceTypes = GetServiceTypes();
         return View(model);
            }

          // Update service
service.ServiceName = model.ServiceName;
          service.Description = model.Description;
            service.BasePrice = model.BasePrice;
            service.EstimatedDuration = model.EstimatedDuration;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} updated for tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "تم تحديث الخدمة بنجاح";

            return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error updating service");
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث الخدمة");
       ViewBag.ServiceTypes = GetServiceTypes();
     return View(model);
    }
    }

    /// <summary>
    /// Delete service
    /// POST: /tailor/manage/services/delete/{id}
    /// </summary>
  [HttpPost("services/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
     var tailor = await GetTailorProfileAsync(userId);

  if (tailor == null)
         return Unauthorized();

       var service = await _context.TailorServices
    .FirstOrDefaultAsync(s => s.TailorServiceId == id && s.TailorId == tailor.Id && !s.IsDeleted);

            if (service == null)
         return NotFound();

// Soft delete
       service.IsDeleted = true;
      await _context.SaveChangesAsync();

        _logger.LogInformation("Service {ServiceId} deleted for tailor {TailorId}", id, tailor.Id);
   TempData["Success"] = "تم حذف الخدمة بنجاح";

         return RedirectToAction(nameof(ManageServices));
        }
    catch (Exception ex)
        {
 _logger.LogError(ex, "Error deleting service");
       TempData["Error"] = "حدث خطأ أثناء حذف الخدمة";
         return RedirectToAction(nameof(ManageServices));
        }
    }

    #endregion

#region Pricing Management

    /// <summary>
    /// Bulk update service prices
    /// GET: /tailor/manage/pricing
    /// </summary>
    [HttpGet("pricing")]
    public async Task<IActionResult> ManagePricing()
    {
  try
        {
   var userId = User.GetUserId();
       var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
     return NotFound("الملف الشخصي غير موجود");

            var services = await _context.TailorServices
     .Where(s => s.TailorId == tailor.Id && !s.IsDeleted)
              .OrderBy(s => s.ServiceName)
       .ToListAsync();

         var model = new ManagePricingViewModel
            {
                TailorId = tailor.Id,
              TailorName = tailor.FullName ?? "خياط",
            ServicePrices = services.Select(s => new ServicePriceDto
           {
  ServiceId = s.TailorServiceId,
        ServiceName = s.ServiceName,
     CurrentPrice = s.BasePrice,
    NewPrice = s.BasePrice
   }).ToList()
            };

  ViewData["Title"] = "إدارة الأسعار";
            return View(model);
        }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Error loading pricing management");
  TempData["Error"] = "حدث خطأ أثناء تحميل إدارة الأسعار";
    return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// <summary>
    /// Update service prices
    /// POST: /tailor/manage/pricing
    /// </summary>
 [HttpPost("pricing")]
    [ValidateAntiForgeryToken]
  public async Task<IActionResult> UpdatePricing(ManagePricingViewModel model)
    {
   try
        {
       var userId = User.GetUserId();
   var tailor = await GetTailorProfileAsync(userId);

     if (tailor == null || tailor.Id != model.TailorId)
          return Unauthorized();

          if (!ModelState.IsValid)
        return View(model);

            // Update service prices
            foreach (var servicePrice in model.ServicePrices)
            {
      var service = await _context.TailorServices
            .FirstOrDefaultAsync(s => s.TailorServiceId == servicePrice.ServiceId && s.TailorId == tailor.Id);

      if (service != null && servicePrice.NewPrice > 0)
            {
        service.BasePrice = servicePrice.NewPrice;
  }
      }

        await _context.SaveChangesAsync();

            _logger.LogInformation("Prices updated for tailor {TailorId}", tailor.Id);
            TempData["Success"] = "تم تحديث الأسعار بنجاح";

  return RedirectToAction(nameof(ManageServices));
        }
 catch (Exception ex)
        {
       _logger.LogError(ex, "Error updating pricing");
        ModelState.AddModelError("", "حدث خطأ أثناء تحديث الأسعار");
            return View(model);
        }
    }

    #endregion

 #region Getting Started

    /// <summary>
    /// Getting started guide for new tailors
    /// GET: /tailor/manage/getting-started
    /// </summary>
    [HttpGet("getting-started")]
    public IActionResult GettingStarted()
    {
        return View();
    }

    #endregion

 #region Helper Methods

    private async Task<TailorProfile?> GetTailorProfileAsync(Guid? userId)
    {
if (!userId.HasValue || userId.Value == Guid.Empty)
     return null;

        return await _context.TailorProfiles
        .Include(t => t.User)
       .FirstOrDefaultAsync(t => t.UserId == userId.Value);
  }

    private List<string> GetPortfolioCategories()
    {
        return new List<string>
  {
            "ثوب رجالي",
   "فستان نسائي",
            "بدلة رسمية",
     "عباءة",
            "جلابية",
    "قميص",
        "تنورة",
     "بنطلون",
    "معطف",
      "أخرى"
   };
    }

 private List<string> GetServiceTypes()
    {
    return new List<string>
    {
      "تفصيل ثوب رجالي",
        "تفصيل فستان نسائي",
    "تفصيل بدلة رسمية",
     "تفصيل عباءة",
        "تفصيل جلابية",
    "تعديل وإصلاح ملابس",
          "تصميم أزياء",
     "خياطة على المقاس",
          "تطريز وزخرفة",
  "خياطة منزلية"
 };
    }

    #endregion
}
