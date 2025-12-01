using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Utility.Extensions;
using TafsilkPlatform.Web.Areas.Tailor.ViewModels.TailorManagement;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Areas.Tailor.Controllers;

/// <summary>
/// Controller for tailor to manage their services, portfolio, and pricing
/// </summary>
[Area("Tailor")]
[Route("manage")]
[Authorize(Roles = "Tailor")]
public partial class TailorManagementController(
    ApplicationDbContext context,
    ILogger<TailorManagementController> logger,
    IFileUploadService fileUploadService,
    ImageUploadService imageUploadService,
    IWebHostEnvironment environment) : Controller
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<TailorManagementController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFileUploadService _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
    private readonly ImageUploadService _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
    private readonly IWebHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    #region Portfolio Management

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
                return NotFound("We couldn't find your profile.");

            var portfolioImages = await _context.PortfolioImages
              .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
                       .OrderByDescending(p => p.IsFeatured)
              .ThenByDescending(p => p.CreatedAt)
             .ToListAsync();

            var model = new ManagePortfolioViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "Tailor",
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

            ViewBag.Categories = GetPortfolioCategories();
            ViewData["Title"] = "Manage Portfolio";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolio management");
            TempData["Error"] = "We couldn't load your portfolio. Please try again.";
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
                TempData["Error"] = "We couldn't load the form. Please refresh the page.";
                return View(new AddPortfolioImageViewModel());
            }

            _logger.LogInformation("AddPortfolioImage POST called for tailor {TailorId}", model.TailorId);

            // Step 1: Validate user and tailor
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor not found for user {UserId}", userId);
                TempData["Error"] = "We couldn't find your profile. Please contact support.";
                return RedirectToAction("Tailor", "Dashboards");
            }

            if (tailor.Id != model.TailorId)
            {
                _logger.LogWarning("Unauthorized access attempt. UserId: {UserId}, TailorId: {TailorId}", userId, model.TailorId);
                TempData["Error"] = "You don't have permission to add images.";
                return RedirectToAction("Tailor", "Dashboards");
            }

            // Step 2: Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for AddPortfolioImage");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Please correct the errors below and try again.";
                return View(model);
            }

            // Step 3: Validate image file
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                _logger.LogWarning("Image file is null or empty");
                ModelState.AddModelError(nameof(model.ImageFile), "Please select an image");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Please upload an image to continue.";
                return View(model);
            }

            // Use best practices validation (extension + MIME + file signature)
            var (isValid, validationError) = await _imageUploadService.ValidateImageAsync(model.ImageFile);
            if (!isValid)
            {
                _logger.LogWarning("Image validation failed: {FileName}, Error: {Error}",
                    model.ImageFile.FileName, validationError);
                ModelState.AddModelError(nameof(model.ImageFile), validationError ?? "Invalid file type");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = validationError ?? "Invalid file type";
                return View(model);
            }

            var maxFileSize = _fileUploadService.GetMaxFileSizeInBytes();
            if (model.ImageFile.Length > maxFileSize)
            {
                _logger.LogWarning("Image file too large: {Size} bytes, Max: {MaxSize} bytes",
                    model.ImageFile.Length, maxFileSize);
                ModelState.AddModelError(nameof(model.ImageFile),
                    $"Image size must be less than {maxFileSize / 1024 / 1024} MB");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Your image is too large. Please upload a file smaller than 10MB.";
                return View(model);
            }

            // Step 4: Check image count limit
            var currentImageCount = await _context.PortfolioImages
                .CountAsync(p => p.TailorId == tailor.Id && !p.IsDeleted);

            if (currentImageCount >= 50)
            {
                _logger.LogWarning("Image limit reached for tailor {TailorId}. Current count: {Count}", tailor.Id, currentImageCount);
                TempData["Error"] = "You've reached the limit of 50 images. Please remove some to add new ones.";
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
                    throw new InvalidOperationException("Failed to read image data. File is empty or corrupt");
                }

                _logger.LogInformation("Image processed successfully. Size: {Size} bytes", imageData.Length);
            }
            catch (OutOfMemoryException oomEx)
            {
                _logger.LogError(oomEx, "Out of memory while reading image. File size: {Size} bytes", model.ImageFile.Length);
                ModelState.AddModelError(nameof(model.ImageFile), "Image is too large. Please choose a smaller image (less than 5MB)");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Image is too large. Please choose a smaller image (less than 5MB)";
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
                ModelState.AddModelError(nameof(model.ImageFile), "Error reading file. Please try again");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Error reading file";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading image data. File size: {Size} bytes, Error: {Error}",
                    model.ImageFile.Length, ex.Message);
                ModelState.AddModelError(nameof(model.ImageFile), "Unexpected error reading image. Please try again");
                ViewBag.Categories = GetPortfolioCategories();
                TempData["Error"] = "Unexpected error reading image";
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
                        throw new InvalidOperationException("Failed to save image to database. No data saved.");
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
                        throw new InvalidOperationException("Failed to verify image save in database");
                    }

                    _logger.LogInformation("✅ Portfolio image {ImageId} added and verified successfully for tailor {TailorId}. ImageSize: {Size} bytes",
                        portfolioImage.PortfolioImageId, tailor.Id, savedImage.ImageData?.Length ?? 0);

                    TempData["Success"] = "Your image has been added to the portfolio!";
                    return (IActionResult)RedirectToAction(nameof(ManagePortfolio));
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
                    ModelState.AddModelError("", "Database error. Please try again.");
                    TempData["Error"] = "Database error. Please check your data and try again.";
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
                    ModelState.AddModelError("", "Unexpected error while saving image. Please try again.");
                    TempData["Error"] = $"An error occurred: {ex.Message}";
                    return (IActionResult)View(model);
                }
            });
        }
        catch (OutOfMemoryException oomEx)
        {
            _logger.LogError(oomEx, "Out of memory exception while adding portfolio image");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "Image is too large. Please choose a smaller image");
            TempData["Error"] = "Image is too large. Please choose a smaller image";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while adding portfolio image");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "Database error. Please try again.");
            TempData["Error"] = "Database error";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (Microsoft.AspNetCore.Http.BadHttpRequestException badReqEx)
        {
            _logger.LogError(badReqEx, "[UPLOAD] BadHttpRequestException during image upload (likely request too large)");
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "File or request size too large. Please choose a smaller image.");
            TempData["Error"] = "File or request size too large. Please choose a smaller image.";
            return View(model ?? new AddPortfolioImageViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UPLOAD] Unexpected error while adding portfolio image. Exception type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, ex.StackTrace);
            ViewBag.Categories = GetPortfolioCategories();
            ModelState.AddModelError("", "Unexpected error. Please try again.");
            TempData["Error"] = $"An error occurred: {ex.Message}";
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
                return NotFound("Image not found");

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
            TempData["Error"] = "We couldn't load the image details.";
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
                if (!await _fileUploadService.IsValidImageAsync(model.NewImageFile))
                {
                    ModelState.AddModelError(nameof(model.NewImageFile), "Invalid file type");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "Invalid file type";
                    return View(model);
                }

                var maxFileSize = _fileUploadService.GetMaxFileSizeInBytes();
                if (model.NewImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(nameof(model.NewImageFile), $"File size too large. Maximum {maxFileSize / 1024 / 1024} MB");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "File size too large";
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
                    ModelState.AddModelError(nameof(model.NewImageFile), "Image is too large. Please choose a smaller image");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "Image is too large. Please choose a smaller image";
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading image data for portfolio image {ImageId}. File size: {Size} bytes",
                        id, model.NewImageFile.Length);
                    ModelState.AddModelError(nameof(model.NewImageFile), "Error reading image. Please try again");
                    ViewBag.Categories = GetPortfolioCategories();
                    TempData["Error"] = "Error reading image";
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio image {ImageId} updated for tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "Your image details have been updated.";

            return RedirectToAction(nameof(ManagePortfolio));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio image");
            ModelState.AddModelError("", "Error updating image");
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
            TempData["Success"] = "The image has been removed from your portfolio.";

            return RedirectToAction(nameof(ManagePortfolio));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portfolio image");
            TempData["Error"] = "We couldn't delete the image. Please try again.";
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

            TempData["Success"] = image.IsFeatured ? "Image is now featured." : "Image is no longer featured.";
            return RedirectToAction(nameof(ManagePortfolio));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured status");
            TempData["Error"] = "We couldn't update the status. Please try again.";
            return RedirectToAction(nameof(ManagePortfolio));
        }
    }

    #endregion

    #region Order Management (إدارة الطلبات)

    /// <summary>
    /// View and manage orders
    /// GET: /tailor/manage/orders
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> ManageOrders(string? search = null, OrderStatus? status = null)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return NotFound("Profile not found");

            // Base query
            var query = _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Where(o => o.TailorId == tailor.Id);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                // search = search.Trim().ToLower(); // Removed ToLower to use Like
                search = search.Trim();
                query = query.Where(o =>
                    o.OrderId.ToString().Contains(search) ||
                    EF.Functions.Like(o.Customer.FullName, $"%{search}%") ||
                    EF.Functions.Like(o.Description, $"%{search}%"));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Get stats (before filtering for accurate counts)
            var allOrders = await _context.Orders
                .Where(o => o.TailorId == tailor.Id)
                .Select(o => new { o.Status, o.TotalPrice, o.CommissionAmount })
                .ToListAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderItemDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = $"#{o.OrderId.ToString().Substring(0, 8).ToUpper()}",
                    CustomerName = o.Customer.FullName,
                    CustomerImageUrl = null, // ProfilePictureData not available on User model
                    Description = o.Description,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    DueDate = o.DueDate,
                    IsNew = o.Status == OrderStatus.Pending,
                    OrderType = o.OrderType
                })
                .ToListAsync();

            var model = new ManageOrdersViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "Tailor",
                Orders = orders,
                TotalOrders = allOrders.Count,
                PendingOrders = allOrders.Count(o => o.Status == OrderStatus.Pending),
                CompletedOrders = allOrders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = allOrders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = allOrders.Where(o => o.Status == OrderStatus.Delivered).Select(o => o.TotalPrice - o.CommissionAmount).DefaultIfEmpty(0).Sum(),
                SearchTerm = search,
                FilterStatus = status
            };

            ViewData["Title"] = "Manage Orders";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders management");
            TempData["Error"] = "We couldn't load your orders.";
            return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// <summary>
    /// Update order status
    /// POST: /tailor/manage/orders/update-status/{id}
    /// </summary>
    [HttpPost("orders/update-status/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, OrderStatus status)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return Unauthorized();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id && o.TailorId == tailor.Id);

            if (order == null)
                return NotFound();

            var oldStatus = order.Status;
            order.Status = status;

            // If status changed to Delivered, update payment status if needed or trigger notifications
            if (status == OrderStatus.Delivered && oldStatus != OrderStatus.Delivered)
            {
                // Logic for delivery completion
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated successfully", newStatus = status.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return Json(new { success = false, message = "Error updating status" });
        }
    }

    /// <summary>
    /// Get order details for modal
    /// GET: /tailor/manage/orders/details/{id}
    /// </summary>
    [HttpGet("orders/details/{id:guid}")]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                .Include(o => o.OrderImages)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.TailorId == tailor.Id);

            if (order == null)
                return NotFound();

            var details = new
            {
                order.OrderId,
                OrderNumber = $"#{order.OrderId.ToString()[..8].ToUpper()}",
                CustomerName = order.Customer.FullName,
                CustomerPhone = order.Customer.User.PhoneNumber,
                CustomerEmail = order.Customer.User.Email,
                order.Description,
                order.TotalPrice,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt.ToString("dd/MM/yyyy"),
                DueDate = order.DueDate?.ToString("dd/MM/yyyy") ?? "Not set",
                order.MeasurementsJson,
                Images = order.OrderImages.Select(i => new { ImageUrl = i.ImgUrl, i.OrderImageId }).ToList(),
                Items = order.Items.Select(i => new { ItemName = i.Description, i.Quantity, Price = i.UnitPrice }).ToList()
            };

            return Json(new { success = true, data = details });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details");
            return Json(new { success = false, message = "Error loading details" });
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
                return NotFound("Profile not found");

            var services = await _context.TailorServices
            .Where(s => s.TailorId == tailor.Id && !s.IsDeleted)
             .OrderBy(s => s.ServiceName)
          .ToListAsync();

            var model = new ManageServicesViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "Tailor",
                Services = services.Select(s => new ServiceItemDto
                {
                    Id = s.TailorServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    BasePrice = (double)s.BasePrice,
                    EstimatedDuration = s.EstimatedDuration.ToString()
                }).ToList(),
                TotalServices = services.Count,
                AveragePrice = (double)(services.Count > 0 ? services.Average(s => s.BasePrice) : 0)
            };

            ViewData["Title"] = "Manage Services";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading services management");
            TempData["Error"] = "We couldn't load your services.";
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
                ModelState.AddModelError(nameof(model.ServiceName), "Service with this name already exists");
                ViewBag.ServiceTypes = GetServiceTypes();
                return View(model);
            }

            // Create service
            var service = new TailorService
            {
                TailorServiceId = Guid.NewGuid(),
                TailorId = tailor.Id,
                ServiceName = model.ServiceName ?? string.Empty,
                Description = model.Description ?? string.Empty,
                BasePrice = (decimal)model.BasePrice,
                EstimatedDuration = int.TryParse(model.EstimatedDuration, out var d) ? d : 0,
                IsDeleted = false
            };

            _context.TailorServices.Add(service);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service added for tailor {TailorId}", tailor.Id);
            TempData["Success"] = "Your new service has been added!";

            return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service");
            ModelState.AddModelError("", "Error adding service");
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
                return NotFound("Service not found");

            var model = new EditServiceViewModel
            {
                Id = service.TailorServiceId,
                TailorId = tailor.Id,
                ServiceName = service.ServiceName,
                Description = service.Description,
                BasePrice = (double)service.BasePrice,
                EstimatedDuration = service.EstimatedDuration.ToString()
            };

            ViewBag.ServiceTypes = GetServiceTypes();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading service for editing");
            TempData["Error"] = "Error loading service";
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
                ModelState.AddModelError(nameof(model.ServiceName), "Service with this name already exists");
                ViewBag.ServiceTypes = GetServiceTypes();
                return View(model);
            }

            // Update service
            service.ServiceName = model.ServiceName ?? string.Empty;
            service.Description = model.Description ?? string.Empty;
            service.BasePrice = (decimal)model.BasePrice;
            service.EstimatedDuration = int.TryParse(model.EstimatedDuration, out var d) ? d : 0;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} updated for tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "Service details have been updated.";

            return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service");
            ModelState.AddModelError("", "Error updating service");
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
            TempData["Success"] = "The service has been removed.";

            return RedirectToAction(nameof(ManageServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service");
            TempData["Error"] = "We couldn't delete the service. Please try again.";
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
                return NotFound("Profile not found");

            var services = await _context.TailorServices
     .Where(s => s.TailorId == tailor.Id && !s.IsDeleted)
              .OrderBy(s => s.ServiceName)
                .ToListAsync();

            var model = new ManagePricingViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "Tailor",
                PricingTier = "Standard",
                StandardServices = services.Select(s => new ServicePriceDto
                {
                    ServiceId = s.TailorServiceId,
                    ServiceName = s.ServiceName,
                    CurrentPrice = (double)s.BasePrice,
                    NewPrice = (double)s.BasePrice
                }).ToList(),
                CustomServices = []
            };

            ViewData["Title"] = "Manage Pricing";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing management");
            TempData["Error"] = "We couldn't load your pricing settings.";
            return RedirectToAction("Tailor", "Dashboards");
        }
    }



    #endregion

    #region Product Management

    /// <summary>
    /// View and manage products
    /// GET: /tailor/manage/products
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> ManageProducts(string? search = null, string? category = null, bool? isAvailable = null, int page = 1)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null)
                return NotFound("Profile not found");

            var query = _context.Products
                .Where(p => p.TailorId == tailor.Id && !p.IsDeleted);

            // Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{search}%") ||
                    EF.Functions.Like(p.Description, $"%{search}%") ||
                    EF.Functions.Like(p.Brand, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == isAvailable.Value);
            }

            // Pagination
            int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductItemDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountedPrice = p.DiscountedPrice,
                    Category = p.Category,
                    StockQuantity = p.StockQuantity,
                    IsAvailable = p.IsAvailable,
                    ImageUrl = p.PrimaryImageData != null && p.PrimaryImageData.Length > 0
                        ? Url.Action("GetProductImage", "TailorManagement", new { id = p.ProductId })
                        : p.PrimaryImageUrl,
                    SalesCount = p.SalesCount,
                    ViewCount = p.ViewCount
                })
                .ToListAsync();

            var model = new ManageProductsViewModel
            {
                TailorId = tailor.Id,
                TailorName = tailor.FullName ?? "Tailor",
                Products = products,
                TotalProducts = totalItems,
                AvailableProducts = await _context.Products.CountAsync(p => p.TailorId == tailor.Id && p.IsAvailable && !p.IsDeleted),
                OutOfStockProducts = await _context.Products.CountAsync(p => p.TailorId == tailor.Id && p.StockQuantity == 0 && !p.IsDeleted),
                TotalInventoryValue = await _context.Products
                    .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
                    .SumAsync(p => p.Price * p.StockQuantity),
                SearchTerm = search,
                FilterCategory = category,
                FilterAvailability = isAvailable,
                CurrentPage = page,
                TotalPages = totalPages
            };

            ViewBag.Categories = GetProductCategories();
            ViewData["Title"] = "Manage Products";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products management");
            TempData["Error"] = "We couldn't load your products.";
            return RedirectToAction("Tailor", "Dashboards");
        }
    }

    /// GET: /tailor/manage/add-product
    /// </summary>
    [HttpGet("add-product")]
    public async Task<IActionResult> AddProduct()
    {
        var userId = User.GetUserId();
        var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
            return NotFound();

        var model = new AddProductViewModel
        {
            TailorId = tailor.Id,
            IsAvailable = true // Default to available
        };

        PopulateProductFormViewBag();
        return View(model);
    }

    /// <summary>
    /// Save new product
    /// POST: /tailor/manage/add-product
    /// </summary>
    [HttpPost("add-product")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB limit
    public async Task<IActionResult> AddProduct(AddProductViewModel model)
    {
        try
        {
            var userId = User.GetUserId();
            var tailor = await GetTailorProfileAsync(userId);

            if (tailor == null || tailor.Id != model.TailorId)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                PopulateProductFormViewBag();
                return View(model);
            }

            // Validate Discounted Price
            if (model.DiscountedPrice.HasValue && model.DiscountedPrice.Value >= model.Price)
            {
                ModelState.AddModelError(nameof(model.DiscountedPrice), "Discounted price must be less than the original price.");
                PopulateProductFormViewBag();
                return View(model);
            }

            // Validate Primary Image
            var (isPrimaryValid, primaryError) = await ValidatePrimaryImageAsync(model.PrimaryImage);
            if (!isPrimaryValid)
            {
                ModelState.AddModelError(nameof(model.PrimaryImage), primaryError!);
                PopulateProductFormViewBag();
                return View(model);
            }

            // Process Primary Image
            byte[] primaryImageData;
            try
            {
                primaryImageData = await ProcessPrimaryImageAsync(model.PrimaryImage!);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.PrimaryImage), ex.Message);
                PopulateProductFormViewBag();
                return View(model);
            }

            // Process Additional Images
            string? additionalImagesJson = await ProcessAdditionalImagesAsync(model.AdditionalImages);

            // Create Entity
            var product = await CreateProductEntityAsync(model, tailor, primaryImageData, additionalImagesJson);

            // Save to DB
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Verify Save
            var (isSaved, savedProduct) = await VerifyProductSavedAsync(product.ProductId, product);
            if (!isSaved)
            {
                TempData["Error"] = "Something went wrong. The product wasn't saved.";
                return RedirectToAction(nameof(ManageProducts));
            }

            _logger.LogInformation("Product {ProductId} added successfully for tailor {TailorId}", product.ProductId, tailor.Id);
            TempData["Success"] = "Your product has been added to the catalog!";

            return RedirectToAction(nameof(ManageProducts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            ModelState.AddModelError("", "Error adding product. Please try again.");
            PopulateProductFormViewBag();
            return View(model);
        }
    }

    /// <summary>
    /// Edit product form
    /// GET: /tailor/manage/edit-product/{id}
    /// </summary>
    [HttpGet("edit-product/{id:guid}")]
    public async Task<IActionResult> EditProduct(Guid id)
    {
        var userId = User.GetUserId();
        var tailor = await GetTailorProfileAsync(userId);

        if (tailor == null)
            return NotFound();

        var product = await _context.Products
            .Include(p => p.Tailor)
            .FirstOrDefaultAsync(p => p.ProductId == id && p.TailorId == tailor.Id && !p.IsDeleted);

        if (product == null)
            return NotFound();

        var model = new EditProductViewModel
        {
            ProductId = product.ProductId,
            TailorId = tailor.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountedPrice = product.DiscountedPrice,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            SubCategory = product.SubCategory,
            Size = product.Size,
            Color = product.Color,
            Material = product.Material,
            Brand = product.Brand,
            IsAvailable = product.IsAvailable,
            IsFeatured = product.IsFeatured,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            HasCurrentPrimaryImage = product.PrimaryImageData != null,
            CurrentAdditionalImagesCount = !string.IsNullOrEmpty(product.AdditionalImagesJson)
                ? product.AdditionalImagesJson.Split(";;", StringSplitOptions.RemoveEmptyEntries).Length
                : 0
        };

        PopulateProductFormViewBag("Edit Product");
        return View(model);
    }

    /// <summary>
    /// Update product
    /// POST: /tailor/manage/edit-product/{id}
    /// </summary>
    [HttpPost("edit-product/{id:guid}")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
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
                PopulateProductFormViewBag("Edit Product");
                return View(model);
            }

            // Validate Discounted Price
            if (model.DiscountedPrice.HasValue && model.DiscountedPrice.Value >= model.Price)
            {
                ModelState.AddModelError(nameof(model.DiscountedPrice), "Discounted price must be less than the original price.");
                PopulateProductFormViewBag("Edit Product");
                return View(model);
            }

            // Update fields
            product.Name = model.Name.Trim();
            product.Description = model.Description.Trim();
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
            product.MetaTitle = model.MetaTitle?.Trim();
            product.MetaDescription = model.MetaDescription?.Trim();
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Handle Primary Image Update
            if (model.NewPrimaryImage != null && model.NewPrimaryImage.Length > 0)
            {
                var (isValid, error) = await ValidatePrimaryImageAsync(model.NewPrimaryImage);
                if (!isValid)
                {
                    ModelState.AddModelError(nameof(model.NewPrimaryImage), error!);
                    PopulateProductFormViewBag("Edit Product");
                    return View(model);
                }

                try
                {
                    product.PrimaryImageData = await ProcessPrimaryImageAsync(model.NewPrimaryImage);
                    product.PrimaryImageContentType = model.NewPrimaryImage.ContentType;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.NewPrimaryImage), ex.Message);
                    PopulateProductFormViewBag("Edit Product");
                    return View(model);
                }
            }

            // Handle Additional Images Update (Append)
            if (model.NewAdditionalImages != null && model.NewAdditionalImages.Count > 0)
            {
                var newImagesJson = await ProcessAdditionalImagesAsync(model.NewAdditionalImages);
                if (!string.IsNullOrEmpty(newImagesJson))
                {
                    if (string.IsNullOrEmpty(product.AdditionalImagesJson))
                    {
                        product.AdditionalImagesJson = newImagesJson;
                    }
                    else
                    {
                        // Append to existing
                        product.AdditionalImagesJson += ";;" + newImagesJson;
                    }
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} updated successfully for tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "Product details have been updated.";

            return RedirectToAction(nameof(ManageProducts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            ModelState.AddModelError("", "Error updating product");
            PopulateProductFormViewBag("Edit Product");
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
                TempData["Error"] = "Cannot delete product because it is linked to active orders";
                return RedirectToAction(nameof(ManageProducts));
            }

            // Soft delete
            product.IsDeleted = true;
            product.IsAvailable = false;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deleted by tailor {TailorId}", id, tailor.Id);
            TempData["Success"] = "Product deleted successfully";

            return RedirectToAction(nameof(ManageProducts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            TempData["Error"] = "Error deleting product";
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
            return Json(new { success = false, message = "An error occurred" });
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
                return BadRequest("Invalid quantity");

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
            return Json(new { success = false, message = "An error occurred" });
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
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            // Check if we have a file-based image URL
            if (!string.IsNullOrEmpty(product.PrimaryImageUrl))
            {
                // If it's a relative path starting with Attachments/, map it to absolute URL path
                var url = product.PrimaryImageUrl.Replace('\\', '/');
                if (!url.StartsWith('/')) url = "/" + url;
                return Redirect(url);
            }

            // Fallback to BLOB if available
            if (product.PrimaryImageData != null)
            {
                return File(product.PrimaryImageData, product.PrimaryImageContentType ?? "image/jpeg");
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product image {ProductId}", id);
            return NotFound();
        }
    }

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


    private static List<string> GetPortfolioCategories()
    {
        return
        [
            "Wedding",
            "Casual",
            "Formal",
            "Traditional",
            "Modern",
            "Other"
        ];
    }

    private static List<string> GetProductCategories()
    {
        return
        [
            "Men's Thobe",
            "Women's Dress",
            "Formal Suit",
            "Abaya",
            "Jalabiya",
            "Shirt",
            "Skirt",
            "Trousers",
            "Coat",
            "Evening Dress",
            "Kids Wear",
            "Accessories",
            "Other"
        ];
    }

    private static List<string> GetProductSubCategories()
    {
        return
        [
            "Men",
            "Women",
            "Kids",
            "Formal",
            "Casual",
            "Sports",
            "Traditional",
            "Modern"
        ];
    }

    private static List<string> GetProductSizes()
    {
        return
        [
            "XS",
            "S",
            "M",
            "L",
            "XL",
            "XXL",
            "XXXL",
            "Free Size"
        ];
    }

    private static List<string> GetProductMaterials()
    {
        return
        [
            "Cotton 100%",
            "Polyester",
            "Silk",
            "Wool",
            "Linen",
            "Mixed",
            "Jeans",
            "Chiffon",
            "Satin",
            "Velvet"
        ];
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Guid.NewGuid().ToString("N")[..8];

        // Remove diacritics and convert to lowercase
        text = text.Trim().ToLowerInvariant();

        // Replace spaces and special characters with hyphens
        text = SlugRegex().Replace(text, "-");

        // Remove leading/trailing hyphens
        text = text.Trim('-');

        // Limit length
        if (text.Length > 50)
            text = text[..50].TrimEnd('-');

        return string.IsNullOrEmpty(text) ? Guid.NewGuid().ToString("N")[..8] : text;
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
        ViewData["Title"] = title ?? "Add New Product";
    }

    /// <summary>
    /// Validates primary image file
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidatePrimaryImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
        {
            return (false, "Primary image is required");
        }

        // Use ImageUploadService for thorough validation (mime + signature)
        var (isValid, error) = await _imageUploadService.ValidateImageAsync(image)
            .ConfigureAwait(false);
        if (!isValid)
            return (false, error ?? "Invalid file type. Please choose an image (JPG, PNG, GIF, WEBP)");

        var maxSize = _fileUploadService.GetMaxFileSizeInBytes();
        if (image.Length > maxSize)
        {
            var maxSizeMB = maxSize / 1024 / 1024;
            return (false, $"Image size is too large. Maximum {maxSizeMB} MB");
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
        while ((read = await fileStream.ReadAsync(buffer.AsMemory())) > 0)
        {
            totalRead += read;
            if (totalRead > maxSize)
            {
                throw new ArgumentException($"Image size is too large. Maximum {maxSize / 1024 / 1024} MB");
            }
            await memoryStream.WriteAsync(buffer.AsMemory(0, read));
        }

        var imageData = memoryStream.ToArray();
        if (imageData == null || imageData.Length == 0)
            throw new InvalidOperationException("Failed to read image data. File might be corrupt");

        return imageData;
    }

    /// <summary>
    /// Processes additional images and returns JSON string
    /// </summary>
    private async Task<string?> ProcessAdditionalImagesAsync(List<IFormFile>? images)
    {
        if (images == null || images.Count == 0)
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

        if (additionalImagesBase64.Count > 0)
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
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
            _logger.LogInformation("Slug already exists, using unique slug: {Slug}", slug);
        }

        // Auto-generate SEO fields if not provided
        var metaTitle = !string.IsNullOrWhiteSpace(model.MetaTitle)
            ? model.MetaTitle.Trim()
            : model.Name.Trim();

        var metaDescription = !string.IsNullOrWhiteSpace(model.MetaDescription)
            ? model.MetaDescription.Trim()
            : (model.Description.Length > 160
                ? model.Description[..160].Trim()
                : model.Description.Trim());

        var productId = Guid.NewGuid();

        return new Product
        {
            ProductId = productId,
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            Price = (decimal)model.Price,
            DiscountedPrice = (decimal?)model.DiscountedPrice,
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



    #region Helpers

    private static List<SelectListItem> GetServiceTypes()
    {
        return
        [
            new SelectListItem { Value = "Alteration", Text = "Alteration" },
            new SelectListItem { Value = "Repair", Text = "Repair" },
            new SelectListItem { Value = "Custom Tailoring", Text = "Custom Tailoring" },
            new SelectListItem { Value = "Measurements", Text = "Measurements" },
            new SelectListItem { Value = "Consultation", Text = "Consultation" }
        ];
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"[^a-z0-9\u0600-\u06FF]+")]
    private static partial System.Text.RegularExpressions.Regex SlugRegex();

    #endregion

}

