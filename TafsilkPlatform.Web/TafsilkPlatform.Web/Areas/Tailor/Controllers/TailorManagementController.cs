using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Utility.Extensions;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Areas.Tailor.Controllers;

/// <summary>
/// Controller for tailor to manage their services, portfolio, and pricing
/// </summary>
[Area("Tailor")]
[Route("manage")]
[Authorize(Roles = "Tailor")]
public class TailorManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TailorManagementController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly ImageUploadService _imageUploadService;
    private readonly IWebHostEnvironment _environment;

    public TailorManagementController(
        ApplicationDbContext context,
        ILogger<TailorManagementController> logger,
        IFileUploadService fileUploadService,
        ImageUploadService imageUploadService,
        IWebHostEnvironment environment)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit for the entire request
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> AddPortfolioImage(AddPortfolioImageViewModel model)
    {
        _logger.LogInformation("[UPLOAD] AddPortfolioImage POST called. Model null: {IsNull}, File present: {HasFile}, FileName: {FileName}, Length: {Length}",
            model == null, model?.ImageFile != null, model?.ImageFile?.FileName, model?.ImageFile?.Length ?? 0);
        try
        {
            // Step 0: Validate model is not null (model binding might have failed)
            if (model == null)
            {
                _logger.LogWarning("Model binding failed - model is null");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "فشل تحميل البيانات. يرجى المحاولة مرة أخرى";
                return View(new AddPortfolioImageViewModel());
            }

            _logger.LogInformation("AddPortfolioImage POST called for tailor {TailorId}", model.TailorId);

            // Step 1: Validate user and tailor
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor not found for user {UserId}", userId);
                TempData["Error"] = "الملف الشخصي غير موجود";
                return RedirectToAction("Tailor", "Dashboards");
            }

            if (tailor.Id != model.TailorId)
            {
                _logger.LogWarning("Unauthorized access attempt. UserId: {UserId}, TailorId: {TailorId}", userId, model.TailorId);
                TempData["Error"] = "غير مصرح لك بإضافة الصور";
                return RedirectToAction("Tailor", "Dashboards");
            }

            // Step 2: Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for AddPortfolioImage");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "يرجى التحقق من البيانات المدخلة وإصلاح الأخطاء";
                return View(model);
            }

            // Step 3: Validate image file
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                _logger.LogWarning("Image file is null or empty");
                ModelState.AddModelError(nameof(model.ImageFile), "يرجى اختيار صورة");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "يرجى اختيار صورة";
                return View(model);
            }

            // Use best practices validation (extension + MIME + file signature)
            var (isValid, validationError) = await _imageUploadService.ValidateImageAsync(model.ImageFile);
            if (!isValid)
            {
                _logger.LogWarning("Image validation failed: {FileName}, Error: {Error}",
                    model.ImageFile.FileName, validationError);
                ModelState.AddModelError(nameof(model.ImageFile), validationError ?? "نوع الملف غير صالح");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = validationError ?? "نوع الملف غير صالح";
                return View(model);
            }

            var maxFileSize = _fileUploadService.GetMaxFileSizeInBytes();
            if (model.ImageFile.Length > maxFileSize)
            {
                _logger.LogWarning("Image file too large: {Size} bytes, Max: {MaxSize} bytes",
                    model.ImageFile.Length, maxFileSize);
                ModelState.AddModelError(nameof(model.ImageFile),
                    $"حجم الصورة يجب أن يكون أقل من {maxFileSize / 1024 / 1024} ميجابايت");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "حجم الصورة كبير جداً";
                return View(model);
            }

            // Step 4: Check image count limit
            var currentImageCount = await _context.PortfolioImages
                .CountAsync(p => p.TailorId == tailor.Id && !p.IsDeleted);

            if (currentImageCount >= 50)
            {
                _logger.LogWarning("Image limit reached for tailor {TailorId}. Current count: {Count}", tailor.Id, currentImageCount);
                TempData["Error"] = "لقد وصلت إلى الحد الأقصى لعدد الصور (50 صورة)";
                return RedirectToAction(nameof(ManagePortfolio));
            }

            // Step 5: Read image data using best practices service
            byte[] imageData;
            try
            {
                _logger.LogInformation("Processing image using best practices. File size: {Size} bytes", model.ImageFile.Length);

                // Use the best practices image processing service
                // This uses memory-efficient buffered streaming and validates during processing
                imageData = await _imageUploadService.ProcessImageAsync(model.ImageFile);

                if (imageData == null || imageData.Length == 0)
                {
                    throw new InvalidOperationException("فشل قراءة بيانات الصورة. الملف فارغ أو تالف");
                }

                _logger.LogInformation("Image processed successfully. Size: {Size} bytes", imageData.Length);
            }
            catch (OutOfMemoryException oomEx)
            {
                _logger.LogError(oomEx, "Out of memory while reading image. File size: {Size} bytes", model.ImageFile.Length);
                ModelState.AddModelError(nameof(model.ImageFile), "الصورة كبيرة جداً. يرجى اختيار صورة أصغر (أقل من 5 ميجابايت)");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "الصورة كبيرة جداً. يرجى اختيار صورة أصغر (أقل من 5 ميجابايت)";
                return View(model);
            }
            catch (InvalidOperationException ioEx)
            {
                _logger.LogError(ioEx, "Invalid operation while reading image. File size: {Size} bytes", model.ImageFile.Length);
                ModelState.AddModelError(nameof(model.ImageFile), ioEx.Message);
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = ioEx.Message;
                return View(model);
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "IO error while reading image. File size: {Size} bytes", model.ImageFile.Length);
                ModelState.AddModelError(nameof(model.ImageFile), "حدث خطأ أثناء قراءة الملف. يرجى المحاولة مرة أخرى");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "حدث خطأ أثناء قراءة الملف";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading image data. File size: {Size} bytes, Error: {Error}",
                    model.ImageFile.Length, ex.Message);
                ModelState.AddModelError(nameof(model.ImageFile), "حدث خطأ غير متوقع أثناء قراءة الصورة. يرجى المحاولة مرة أخرى");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "حدث خطأ غير متوقع أثناء قراءة الصورة";
                return View(model);
            }

            // Step 6: Get next display order
            var maxOrder = await _context.PortfolioImages
                .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
                .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

            // Step 7: Create portfolio image entity
            var portfolioImage = new PortfolioImage
            {
                PortfolioImageId = Guid.NewGuid(),
                TailorId = tailor.Id,
                Title = model.Title?.Trim(),
                Category = model.Category,
                Description = model.Description?.Trim(),
                EstimatedPrice = model.EstimatedPrice,
                IsFeatured = model.IsFeatured,
                IsBeforeAfter = model.IsBeforeAfter,
                DisplayOrder = maxOrder + 1,
                ImageData = imageData,
                ContentType = model.ImageFile.ContentType ?? "image/jpeg",
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _logger.LogInformation("Portfolio image entity created. ImageId: {ImageId}, TailorId: {TailorId}, ImageSize: {Size} bytes",
                portfolioImage.PortfolioImageId, tailor.Id, imageData.Length);

            // Step 8: Save to database with transaction using execution strategy
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.PortfolioImages.Add(portfolioImage);
                    _logger.LogInformation("Portfolio image added to context. Starting database save...");

                    var saveResult = await _context.SaveChangesAsync();
                    _logger.LogInformation("SaveChangesAsync completed. Result: {SaveResult} entities saved", saveResult);

                    if (saveResult == 0)
                    {
                        _logger.LogError("CRITICAL: Failed to save portfolio image - SaveChangesAsync returned 0");
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException("فشل حفظ الصورة في قاعدة البيانات. لم يتم حفظ أي بيانات.");
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully. ImageId: {ImageId}", portfolioImage.PortfolioImageId);

                    // Step 9: Verify the portfolio image was actually saved
                    var savedImage = await _context.PortfolioImages
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PortfolioImageId == portfolioImage.PortfolioImageId && !p.IsDeleted);

                    if (savedImage == null)
                    {
                        _logger.LogError("CRITICAL: Portfolio image was not found in database after commit. ImageId: {ImageId}",
                            portfolioImage.PortfolioImageId);
                        throw new InvalidOperationException("فشل التحقق من حفظ الصورة في قاعدة البيانات");
                    }

                    _logger.LogInformation("✅ Portfolio image {ImageId} added and verified successfully for tailor {TailorId}. ImageSize: {Size} bytes",
                        portfolioImage.PortfolioImageId, tailor.Id, savedImage.ImageData?.Length ?? 0);

                    TempData["Success"] = "تم إضافة الصورة بنجاح إلى معرض الأعمال";
                    return (IActionResult)RedirectToAction("Tailor", "Dashboards");
                }
                catch (DbUpdateException dbEx)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Error during transaction rollback");
                    }

                    _logger.LogError(dbEx, "Database update exception during portfolio image save");

                    ViewBag.Categories = GetPortfolioCategories();
                    ModelState.AddModelError("", "حدث خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى.");
                    TempData["Error"] = "حدث خطأ في قاعدة البيانات. يرجى التحقق من البيانات والمحاولة مرة أخرى.";
                    return (IActionResult)View(model);
                }
                catch (InvalidOperationException ioEx)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Error during transaction rollback");
                    }

                    _logger.LogError(ioEx, "Invalid operation during portfolio image save");

                    ViewBag.Categories = GetPortfolioCategories();
                    ModelState.AddModelError("", ioEx.Message);
                    TempData["Error"] = ioEx.Message;
                    return (IActionResult)View(model);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Error during transaction rollback");
                    }

                    _logger.LogError(ex, "Unexpected error during portfolio image save transaction");

                    ViewBag.Categories = GetPortfolioCategories();
                    ModelState.AddModelError("", "حدث خطأ غير متوقع أثناء حفظ الصورة. يرجى المحاولة مرة أخرى.");
                    TempData["Error"] = $"حدث خطأ: {ex.Message}";
                    return (IActionResult)View(model);
                }
            });
        }
        catch (OutOfMemoryException oomEx)
        {
            _logger.LogError(oomEx, "Out of memory exception while adding portfolio image");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "الصورة كبيرة جداً. يرجى اختيار صورة أصغر");
            TempData["Error"] = "الصورة كبيرة جداً. يرجى اختيار صورة أصغر";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while adding portfolio image");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "حدث خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى.");
            TempData["Error"] = "حدث خطأ في قاعدة البيانات";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (Microsoft.AspNetCore.Http.BadHttpRequestException badReqEx)
        {
            _logger.LogError(badReqEx, "[UPLOAD] BadHttpRequestException during image upload (likely request too large)");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "حجم الملف أو الطلب كبير جداً. يرجى اختيار صورة أصغر أو تقليل عدد الملفات.");
            TempData["Error"] = "حجم الملف أو الطلب كبير جداً. يرجى اختيار صورة أصغر أو تقليل عدد الملفات.";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UPLOAD] Unexpected error while adding portfolio image. Exception type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, ex.StackTrace);
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.");
            TempData["Error"] = $"حدث خطأ: {ex.Message}";
            return View(model ?? new AddPortfolioImageViewModel());
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
                    TempData["Error"] = "نوع الملف غير صالح";
                    return View(model);
                }

                var maxFileSize = _fileUploadService.GetMaxFileSizeInBytes();
                if (model.NewImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(nameof(model.NewImageFile), $"حجم الملف كبير جداً. الحد الأقصى {maxFileSize / 1024 / 1024} ميجابايت");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "حجم الملف كبير جداً";
                    return View(model);
                }

                // Read image data with proper error handling
                try
                {
                    _logger.LogInformation("Reading new image data for portfolio image {ImageId}. File size: {Size} bytes",
                        id, model.NewImageFile.Length);

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.NewImageFile.CopyToAsync(memoryStream);
                        image.ImageData = memoryStream.ToArray();
                    }

                    image.ContentType = model.NewImageFile.ContentType ?? "image/jpeg";
                    _logger.LogInformation("New image data read successfully. Size: {Size} bytes", image.ImageData.Length);
                }
                catch (OutOfMemoryException oomEx)
                {
                    _logger.LogError(oomEx, "Out of memory while reading image for portfolio image {ImageId}. File size: {Size} bytes",
                        id, model.NewImageFile.Length);
                    ModelState.AddModelError(nameof(model.NewImageFile), "الصورة كبيرة جداً. يرجى اختيار صورة أصغر");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "الصورة كبيرة جداً. يرجى اختيار صورة أصغر";
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading image data for portfolio image {ImageId}. File size: {Size} bytes",
                        id, model.NewImageFile.Length);
                    ModelState.AddModelError(nameof(model.NewImageFile), "حدث خطأ أثناء قراءة الصورة. يرجى المحاولة مرة أخرى");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "حدث خطأ أثناء قراءة الصورة";
                    return View(model);
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

            if (image == null)
                return NotFound();

            // Prefer binary data if available
            if (image.ImageData != null && image.ImageData.Length > 0)
            {
                return File(image.ImageData, image.ContentType ?? "image/jpeg");
            }

            // Fallback to ImageUrl if present
            if (!string.IsNullOrWhiteSpace(image.ImageUrl))
            {
                // Absolute URL -> redirect
                if (Uri.IsWellFormedUriString(image.ImageUrl, UriKind.Absolute))
                {
                    return Redirect(image.ImageUrl);
                }

                // Relative URL (starts with '/') -> serve physical file from wwwroot
                if (image.ImageUrl.StartsWith('/'))
                {
                    var relativePath = image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var physicalPath = Path.Combine(_environment.WebRootPath ?? string.Empty, relativePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        var contentType = image.ContentType ?? "image/jpeg";
                        return PhysicalFile(physicalPath, contentType);
                    }
                }

                // As a last resort, try to return redirect to whatever value was stored
                return Redirect(image.ImageUrl);
            }

            return NotFound();
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

    #region Product Management (إدارة المنتجات)

    /// <summary>
    /// View and manage products
    /// GET: /tailor/manage/products
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> ManageProducts()
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return NotFound("الملف الشخصي غير موجود");

            var products = await _context.Products
                .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var model = new ManageProductsViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "خياط",
                Products = products.Select(p => new ProductItemDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    DiscountedPrice = p.DiscountedPrice,
                    StockQuantity = p.StockQuantity,
                    IsAvailable = p.IsAvailable,
                    IsFeatured = p.IsFeatured,
                    ViewCount = p.ViewCount,
                    SalesCount = p.SalesCount,
                    AverageRating = p.AverageRating,
                    CreatedAt = p.CreatedAt,
                    HasImage = p.PrimaryImageData != null
                }).ToList(),
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.IsAvailable && p.StockQuantity > 0),
                OutOfStockProducts = products.Count(p => p.StockQuantity == 0),
                TotalInventoryValue = products.Sum(p => p.Price * p.StockQuantity)
            };

            ViewData["Title"] = "إدارة المنتجات";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products management");
            TempData["Error"] = "حدث خطأ أثناء تحميل المنتجات";
            return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// <summary>
    /// Add new product form
    /// GET: /tailor/manage/products/add
    /// </summary>
    [HttpGet("products/add")]
    public async Task<IActionResult> AddProduct()
    {
        var userId = User.GetUserId();
        var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
            return NotFound();

        var model = new AddProductViewModel
        {
            TailorId = tailor.Id,
            IsAvailable = true,
            StockQuantity = 1
        };

        PopulateProductFormViewBag();
        return View(model);
    }

    /// <summary>
    /// Save new product
    /// POST: /tailor/manage/products/add
    /// </summary>
    [HttpPost("products/add")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 12 * 1024 * 1024, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> AddProduct(AddProductViewModel? model)
    {
        try
        {
            if (model == null)
            {
                PopulateProductFormViewBag();
                TempData["Error"] = "فشل تحميل البيانات. يرجى المحاولة مرة أخرى";
                return View(new AddProductViewModel());
            }
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);
            if (tailor == null)
            {
                TempData["Error"] = "الملف الشخصي غير موجود";
                return RedirectToAction("Tailor", "Dashboards");
            }
            if (tailor.Id != model.TailorId)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid)
            {
                PopulateProductFormViewBag();
                TempData["Error"] = "يرجى التحقق من البيانات المدخلة وإصلاح الأخطاء";
                return View(model);
            }
            int validImageCount = 0;
            var (isImageValid, imageError) = await _imageUploadService.ValidateImageAsync(model.PrimaryImage);
            if (!isImageValid)
            {
                ModelState.AddModelError(nameof(model.PrimaryImage), imageError!);
                PopulateProductFormViewBag();
                TempData["Error"] = imageError;
                return View(model);
            }
            byte[] primaryImageData = Array.Empty<byte>();
            try
            {
                primaryImageData = await _imageUploadService.ProcessImageWithSizeCheckAsync(model.PrimaryImage!);
                validImageCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing primary image");
                ModelState.AddModelError(nameof(model.PrimaryImage), "حدث خطأ أثناء معالجة الصورة. يرجى المحاولة مرة أخرى");
                PopulateProductFormViewBag();
                TempData["Error"] = "حدث خطأ أثناء معالجة الصورة";
                return View(model);
            }
            var additionalImagesJson = await ProcessAdditionalImagesAsync(model.AdditionalImages);
            if (validImageCount == 0)
            {
                TempData["Warning"] = "لم يتم قبول أي صورة من الصور المرفوعة. يرجى التأكد من نوع وحجم الصور.";
            }
            var product = await CreateProductEntityAsync(model, tailor, primaryImageData, additionalImagesJson);
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Products.Add(product);
                    var saveResult = await _context.SaveChangesAsync();
                    if (saveResult == 0)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException("فشل حفظ المنتج في قاعدة البيانات. لم يتم حفظ أي بيانات.");
                    }
                    await transaction.CommitAsync();
                    TempData["Success"] = $"تم إضافة المنتج '{product.Name}' بنجاح وهو الآن متاح في المتجر";
                    return (IActionResult)RedirectToAction("Tailor", "Dashboards");
                }
                catch (Exception ex)
                {
                    try { await transaction.RollbackAsync(); } catch { }
                    _logger.LogError(ex, "Error during product save transaction");
                    PopulateProductFormViewBag();
                    ModelState.AddModelError("", "حدث خطأ غير متوقع أثناء حفظ المنتج. يرجى المحاولة مرة أخرى.");
                    TempData["Error"] = $"حدث خطأ: {ex.Message}";
                    return (IActionResult)View(model);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during product image upload");
            TempData["Error"] = "حدث خطأ أثناء رفع الصورة. يرجى المحاولة مرة أخرى أو اختيار صورة أصغر.";
            PopulateProductFormViewBag();
            return View(model ?? new AddProductViewModel());
        }
    }

    /// <summary>
    /// Edit product
    /// GET: /tailor/manage/products/edit/{id}
    /// </summary>
    [HttpGet("products/edit/{id:guid}")]
    public async Task<IActionResult> EditProduct(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

            if (product == null)
                return NotFound("المنتج غير موجود");

            var additionalImagesCount = 0;
            if (!string.IsNullOrEmpty(product.AdditionalImagesJson))
            {
                additionalImagesCount = product.AdditionalImagesJson.Split(";;").Length;
            }

            var model = new EditProductViewModel
            {
                ProductId = product.ProductId,
                TailorId = tailor.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountedPrice = product.DiscountedPrice,
                Category = product.Category,
                SubCategory = product.SubCategory,
                Size = product.Size,
                Color = product.Color,
                Material = product.Material,
                Brand = product.Brand,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable,
                IsFeatured = product.IsFeatured,
                MetaTitle = product.MetaTitle,
                MetaDescription = product.MetaDescription,
                HasCurrentPrimaryImage = product.PrimaryImageData != null,
                CurrentAdditionalImagesCount = additionalImagesCount
            };

            PopulateProductFormViewBag("تعديل المنتج");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product for editing");
            TempData["Error"] = "حدث خطأ أثناء تحميل المنتج";
            return RedirectToAction(nameof(ManageProducts));
        }
    }

    /// <summary>
    /// Update product
    /// POST: /tailor/manage/products/edit/{id}
    /// </summary>
    [HttpPost("products/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Guid id, EditProductViewModel model)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null || tailor.Id != model.TailorId)
                return Unauthorized();

            if (id != model.ProductId)
                return BadRequest();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateProductFormViewBag("تعديل المنتج");
                return View(model);
            }

            // Update product info
            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.DiscountedPrice = model.DiscountedPrice;
            product.Category = model.Category;
            product.SubCategory = model.SubCategory?.Trim();
            product.Size = model.Size?.Trim();
            product.Color = model.Color?.Trim();
            product.Material = model.Material?.Trim();
            product.Brand = model.Brand?.Trim();
            product.StockQuantity = model.StockQuantity >= 0 ? model.StockQuantity : 0;
            product.IsAvailable = model.IsAvailable;
            product.IsFeatured = model.IsFeatured;
            product.MetaTitle = model.MetaTitle ?? model.Name;
            product.MetaDescription = model.MetaDescription;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Update primary image if provided
            if (model.NewPrimaryImage != null && model.NewPrimaryImage.Length > 0)
            {
                if (!_fileUploadService.IsValidImage(model.NewPrimaryImage))
                {
                    ModelState.AddModelError(nameof(model.NewPrimaryImage), "نوع الملف غير صالح");
                    PopulateProductFormViewBag("تعديل المنتج");
                    return View(model);
                }

                using var ms = new MemoryStream();
                await model.NewPrimaryImage.CopyToAsync(ms);
                product.PrimaryImageData = ms.ToArray();
                product.PrimaryImageContentType = model.NewPrimaryImage.ContentType;
            }

            // Add new additional images if provided
            if (model.NewAdditionalImages != null && model.NewAdditionalImages.Any())
            {
                var existingImages = new List<string>();
                if (!string.IsNullOrEmpty(product.AdditionalImagesJson))
                {
                    existingImages = product.AdditionalImagesJson.Split(";;").ToList();
                }

                foreach (var image in model.NewAdditionalImages)
                {
                    if (existingImages.Count >= 5) break; // Max 5 total

                    if (_fileUploadService.IsValidImage(image))
                    {
                        using var ms = new MemoryStream();
                        await image.CopyToAsync(ms);
                        var base64 = Convert.ToBase64String(ms.ToArray());
                        existingImages.Add($"{image.ContentType}|{base64}");
                    }
                }

                product.AdditionalImagesJson = existingImages.Any() ? string.Join(";;", existingImages) : null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} updated by tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "تم تحديث المنتج بنجاح";

            return RedirectToAction(nameof(ManageProducts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            ModelState.AddModelError("", "حدث خطأ أثناء تحديث المنتج");
            PopulateProductFormViewBag("تعديل المنتج");
            return View(model);
        }
    }

    /// <summary>
    /// Delete product
    /// POST: /tailor/manage/products/delete/{id}
    /// </summary>
    [HttpPost("products/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return Unauthorized();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            // Check if product has active orders
            var hasActiveOrders = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == id &&
                    oi.Order.Status != OrderStatus.Cancelled &&
                    oi.Order.Status != OrderStatus.Delivered);

            if (hasActiveOrders)
            {
                TempData["Error"] = "لا يمكن حذف المنتج لأنه مرتبط بطلبات نشطة";
                return RedirectToAction(nameof(ManageProducts));
            }

            // Soft delete
            product.IsDeleted = true;
            product.IsAvailable = false;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deleted by tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "تم حذف المنتج بنجاح";

            return RedirectToAction(nameof(ManageProducts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            TempData["Error"] = "حدث خطأ أثناء حذف المنتج";
            return RedirectToAction(nameof(ManageProducts));
        }
    }

    /// <summary>
    /// Toggle product availability
    /// POST: /tailor/manage/products/toggle-availability/{id}
    /// </summary>
    [HttpPost("products/toggle-availability/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductAvailability(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return Unauthorized();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            product.IsAvailable = !product.IsAvailable;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isAvailable = product.IsAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling product availability");
            return Json(new { success = false, message = "حدث خطأ" });
        }
    }

    /// <summary>
    /// Quick stock update
    /// POST: /tailor/manage/products/update-stock/{id}
    /// </summary>
    [HttpPost("products/update-stock/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(Guid id, [FromForm] int newStock)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return Unauthorized();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            if (newStock < 0 || newStock > 10000)
                return BadRequest("الكمية غير صالحة");

            product.StockQuantity = newStock;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Auto-update availability based on stock
            if (newStock == 0 && product.IsAvailable)
            {
                product.IsAvailable = false;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, newStock = product.StockQuantity, isAvailable = product.IsAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock");
            return Json(new { success = false, message = "حدث خطأ" });
        }
    }

    /// <summary>
    /// Get product image
    /// GET: /tailor/manage/products/image/{id}
    /// </summary>
    [HttpGet("products/image/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductImage(Guid id)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null || product.PrimaryImageData == null)
                return NotFound();

            return File(product.PrimaryImageData, product.PrimaryImageContentType ?? "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product image {ProductId}", id);
            return NotFound();
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

    private List<string> GetProductCategories()
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
            "فستان سهرة",
            "ملابس أطفال",
            "اكسسوارات",
            "أخرى"
        };
    }

    private List<string> GetProductSubCategories()
    {
        return new List<string>
        {
            "رجالي",
            "نسائي",
            "أطفال",
            "رسمي",
            "كاجوال",
            "رياضي",
            "تقليدي",
            "عصري"
        };
    }

    private List<string> GetProductSizes()
    {
        return new List<string>
        {
            "XS",
            "S",
            "M",
            "L",
            "XL",
            "XXL",
            "XXXL",
            "مقاس حر"
        };
    }

    private List<string> GetProductMaterials()
    {
        return new List<string>
        {
            "قطن 100%",
            "بوليستر",
            "حرير",
            "صوف",
            "كتان",
            "مخلوط",
            "جينز",
            "شيفون",
            "ساتان",
            "قطيفة"
        };
    }

    private string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Guid.NewGuid().ToString("N").Substring(0, 8);

        // Remove diacritics and convert to lowercase
        text = text.Trim().ToLowerInvariant();

        // Replace spaces and special characters with hyphens
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\u0600-\u06FF]+", "-");

        // Remove leading/trailing hyphens
        text = text.Trim('-');

        // Limit length
        if (text.Length > 50)
            text = text.Substring(0, 50).TrimEnd('-');

        return string.IsNullOrEmpty(text) ? Guid.NewGuid().ToString("N").Substring(0, 8) : text;
    }

    /// <summary>
    /// Populates ViewBag with product form data (categories, sizes, materials, etc.)
    /// </summary>
    private void PopulateProductFormViewBag(string? title = null)
    {
        ViewBag.Categories = GetProductCategories();
        ViewBag.SubCategories = GetProductSubCategories();
        ViewBag.Sizes = GetProductSizes();
        ViewBag.Materials = GetProductMaterials();
        ViewData["Title"] = title ?? "إضافة منتج جديد";
    }

    /// <summary>
    /// Validates primary image file
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidatePrimaryImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
        {
            return (false, "الصورة الأساسية مطلوبة");
        }

        // Use ImageUploadService for thorough validation (mime + signature)
        var (isValid, error) = await _imageUploadService.ValidateImageAsync(image)
            .ConfigureAwait(false);
        if (!isValid)
            return (false, error ?? "نوع الملف غير صالح. يرجى اختيار صورة (JPG, PNG, GIF, WEBP)");

        var maxSize = _fileUploadService.GetMaxFileSizeInBytes();
        if (image.Length > maxSize)
        {
            var maxSizeMB = maxSize / 1024 / 1024;
            return (false, $"حجم الصورة كبير جداً. الحد الأقصى {maxSizeMB} ميجابايت");
        }

        return (true, null);
    }

    /// <summary>
    /// Processes and reads primary image data using best practices
    /// Uses memory-efficient streaming with size validation
    /// </summary>
    private async Task<byte[]> ProcessPrimaryImageAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentException("Image file is null or empty");
        }

        // Stream with size check to avoid OOM
        var maxSize = _fileUploadService.GetMaxFileSizeInBytes();
        const int bufferSize = 81920; // 80KB
        long totalRead = 0;

        using var memoryStream = new MemoryStream();
        using var fileStream = image.OpenReadStream();
        var buffer = new byte[bufferSize];
        int read;
        while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            totalRead += read;
            if (totalRead > maxSize)
            {
                throw new ArgumentException($"حجم الصورة كبير جداً. الحد الأقصى {maxSize / 1024 / 1024} ميجابايت");
            }
            await memoryStream.WriteAsync(buffer, 0, read);
        }

        var imageData = memoryStream.ToArray();
        if (imageData == null || imageData.Length == 0)
            throw new InvalidOperationException("فشل قراءة بيانات الصورة. الملف قد يكون تالفاً");

        return imageData;
    }

    /// <summary>
    /// Processes additional images and returns JSON string
    /// </summary>
    private async Task<string?> ProcessAdditionalImagesAsync(List<IFormFile>? images)
    {
        if (images == null || !images.Any())
            return null;

        _logger.LogInformation("Processing {Count} additional images", images.Count);

        var additionalImagesBase64 = new List<string>();
        foreach (var image in images.Take(5)) // Max 5 additional images
        {
            try
            {
                var (isValid, validationError) = await _imageUploadService.ValidateImageAsync(image);
                if (!isValid)
                {
                    _logger.LogWarning("Skipping additional image due to validation: {FileName}, Error: {Error}", image.FileName, validationError);
                    continue;
                }

                if (image.Length > _fileUploadService.GetMaxFileSizeInBytes())
                {
                    _logger.LogWarning("Skipping additional image due to size: {FileName}, Size: {Size}", image.FileName, image.Length);
                    continue;
                }

                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                additionalImagesBase64.Add($"{image.ContentType}|{base64}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing additional image {FileName} - skipping", image?.FileName);
            }
        }

        if (additionalImagesBase64.Any())
        {
            _logger.LogInformation("Processed {Count} additional images successfully", additionalImagesBase64.Count);
            return string.Join(";;", additionalImagesBase64);
        }

        return null;
    }

    /// <summary>
    /// Creates a Product entity from the view model
    /// </summary>
    private async Task<Product> CreateProductEntityAsync(AddProductViewModel model, TailorProfile tailor,
        byte[] primaryImageData, string? additionalImagesJson)
    {
        // Generate unique slug
        var slug = GenerateSlug(model.Name);
        var existingSlug = await _context.Products.AnyAsync(p => p.Slug == slug && !p.IsDeleted);
        if (existingSlug)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            _logger.LogInformation("Slug already exists, using unique slug: {Slug}", slug);
        }

        // Auto-generate SEO fields if not provided
        var metaTitle = !string.IsNullOrWhiteSpace(model.MetaTitle)
            ? model.MetaTitle.Trim()
            : model.Name.Trim();

        var metaDescription = !string.IsNullOrWhiteSpace(model.MetaDescription)
            ? model.MetaDescription.Trim()
            : (model.Description.Length > 160
                ? model.Description.Substring(0, 160).Trim()
                : model.Description.Trim());

        var productId = Guid.NewGuid();

        return new Product
        {
            ProductId = productId,
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            Price = model.Price,
            DiscountedPrice = model.DiscountedPrice,
            Category = model.Category,
            SubCategory = model.SubCategory?.Trim(),
            Size = model.Size?.Trim(),
            Color = model.Color?.Trim(),
            Material = model.Material?.Trim(),
            Brand = model.Brand?.Trim(),
            StockQuantity = model.StockQuantity >= 0 ? model.StockQuantity : 0,
            IsAvailable = model.IsAvailable,
            IsFeatured = model.IsFeatured,
            PrimaryImageData = primaryImageData,
            PrimaryImageContentType = model.PrimaryImage!.ContentType ?? "image/jpeg",
            AdditionalImagesJson = additionalImagesJson,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            Slug = slug,
            TailorId = tailor.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false,
            ViewCount = 0,
            SalesCount = 0,
            AverageRating = 0.0,
            ReviewCount = 0
        };
    }

    /// <summary>
    /// Verifies that the product was saved correctly to the database
    /// </summary>
    private async Task<(bool IsValid, Product? SavedProduct)> VerifyProductSavedAsync(Guid productId, Product originalProduct)
    {
        var savedProduct = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

        if (savedProduct == null)
        {
            _logger.LogError("CRITICAL: Product was not found in database after commit. ProductId: {ProductId}", productId);
            return (false, null);
        }

        // Verify critical fields
        var verificationPassed = savedProduct.Name == originalProduct.Name &&
                               savedProduct.Description == originalProduct.Description &&
                               savedProduct.Price == originalProduct.Price &&
                               savedProduct.TailorId == originalProduct.TailorId &&
                               savedProduct.Category == originalProduct.Category &&
                               savedProduct.PrimaryImageData != null &&
                               savedProduct.PrimaryImageData.Length > 0;

        if (!verificationPassed)
        {
            _logger.LogError("CRITICAL: Product verification failed. Some fields may not have been saved correctly. ProductId: {ProductId}", productId);
            return (false, savedProduct);
        }

        return (true, savedProduct);
    }

    #endregion
}
