using BLL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for tailor portfolio management operations
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        IAttachmentService attachmentService,
        ILogger<PortfolioService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManagePortfolioViewModel?> GetTailorPortfolioAsync(Guid tailorId)
    {
        try
        {
            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
            {
                _logger.LogWarning("Tailor {TailorId} not found", tailorId);
                return null;
            }

            var portfolioImages = await _unitOfWork.Context.PortfolioImages
                .Where(p => p.TailorId == tailorId && !p.IsDeleted)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return new ManagePortfolioViewModel
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
                    HasImageData = p.ImageData != null || !string.IsNullOrEmpty(p.ImageUrl)
                }).ToList(),
                TotalImages = portfolioImages.Count,
                FeaturedCount = portfolioImages.Count(p => p.IsFeatured),
                MaxAllowedImages = 50
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio for tailor {TailorId}", tailorId);
            return null;
        }
    }

    public async Task<(bool Success, Guid? ImageId, string? ErrorMessage)> AddPortfolioImageAsync(
        Guid tailorId,
        AddPortfolioImageViewModel model)
    {
        try
        {
            _logger.LogInformation("Adding portfolio image for tailor {TailorId}", tailorId);

            // Validate tailor exists
            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
            {
                return (false, null, "Profile not found");
            }

            // Validate image file
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return (false, null, "Please select an image");
            }

            if (!await _fileUploadService.IsValidImageAsync(model.ImageFile))
            {
                return (false, null, "Invalid file type. Please select an image (JPG, PNG, GIF)");
            }

            if (model.ImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
            {
                return (false, null, $"Image size must be less than {_fileUploadService.GetMaxFileSizeInBytes() / 1024 / 1024} MB");
            }

            // Check image count limit
            var currentImageCount = await _unitOfWork.Context.PortfolioImages
                .CountAsync(p => p.TailorId == tailorId && !p.IsDeleted);

            if (currentImageCount >= 50)
            {
                return (false, null, "You have reached the maximum number of images (50 images)");
            }

            // Upload to filesystem (gallery folder)
            var uploadedRelative = await _attachmentService.Upload(model.ImageFile, "gallery");
            if (string.IsNullOrEmpty(uploadedRelative))
            {
                return (false, null, "Failed to upload file or file type not supported");
            }

            // Get next display order
            var maxOrder = await _unitOfWork.Context.PortfolioImages
                .Where(p => p.TailorId == tailorId && !p.IsDeleted)
                .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

            // Create portfolio image record referencing filesystem URL
            var portfolioImage = new PortfolioImage
            {
                PortfolioImageId = Guid.NewGuid(),
                TailorId = tailorId,
                Title = model.Title,
                Category = model.Category,
                Description = model.Description,
                EstimatedPrice = model.EstimatedPrice,
                IsFeatured = model.IsFeatured,
                IsBeforeAfter = model.IsBeforeAfter,
                DisplayOrder = maxOrder + 1,
                ImageData = null,
                ImageUrl = uploadedRelative, // e.g. Attachments/gallery/uuid.jpg
                ContentType = model.ImageFile.ContentType,
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // Use UnitOfWork transaction management
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.PortfolioImages.AddAsync(portfolioImage);

                _logger.LogInformation("Saving portfolio image to database");

                var saveResult = await _unitOfWork.SaveChangesAsync();

                if (saveResult == 0)
                {
                    _logger.LogError("Failed to save portfolio image - SaveChangesAsync returned 0");
                    throw new InvalidOperationException("Failed to save image to database");
                }

                _logger.LogInformation(
                    "✅ Portfolio image {ImageId} added successfully for tailor {TailorId}",
                    portfolioImage.PortfolioImageId, tailorId);

                return (true, portfolioImage.PortfolioImageId, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding portfolio image for tailor {TailorId}", tailorId);
            return (false, null, $"Error: {ex.Message}");
        }
    }

    public async Task<EditPortfolioImageViewModel?> GetPortfolioImageForEditAsync(Guid imageId, Guid tailorId)
    {
        try
        {
            var image = await _unitOfWork.Context.PortfolioImages
                .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && p.TailorId == tailorId && !p.IsDeleted);

            if (image == null)
            {
                return null;
            }

            return new EditPortfolioImageViewModel
            {
                Id = image.PortfolioImageId,
                TailorId = tailorId,
                Title = image.Title,
                Category = image.Category,
                Description = image.Description,
                EstimatedPrice = image.EstimatedPrice,
                IsFeatured = image.IsFeatured,
                IsBeforeAfter = image.IsBeforeAfter,
                DisplayOrder = image.DisplayOrder,
                HasCurrentImage = image.ImageData != null || !string.IsNullOrEmpty(image.ImageUrl)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio image {ImageId} for edit", imageId);
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdatePortfolioImageAsync(
        Guid imageId,
        Guid tailorId,
        EditPortfolioImageViewModel model)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var image = await _unitOfWork.Context.PortfolioImages
                    .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && p.TailorId == tailorId && !p.IsDeleted);

                if (image == null)
                {
                    return (false, "Image not found");
                }

                // Update image info
                image.Title = model.Title;
                image.Category = model.Category;
                image.Description = model.Description;
                image.EstimatedPrice = model.EstimatedPrice;
                image.IsFeatured = model.IsFeatured;
                image.IsBeforeAfter = model.IsBeforeAfter;
                image.DisplayOrder = model.DisplayOrder;

                // Update image file if new file provided
                if (model.NewImageFile != null && model.NewImageFile.Length > 0)
                {
                    if (!await _fileUploadService.IsValidImageAsync(model.NewImageFile))
                    {
                        return (false, "Invalid file type");
                    }

                    if (model.NewImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
                    {
                        return (false, "File size is too large");
                    }

                    // Upload new file
                    var newRelative = await _attachmentService.Upload(model.NewImageFile, "gallery");
                    if (string.IsNullOrEmpty(newRelative))
                    {
                        return (false, "Failed to upload new file");
                    }

                    // Delete old file if it exists and was stored in attachments
                    if (!string.IsNullOrEmpty(image.ImageUrl))
                    {
                        try { await _attachmentService.Delete(image.ImageUrl); } catch { /* ignore */ }
                    }

                    image.ImageUrl = newRelative;
                    image.ImageData = null;
                    image.ContentType = model.NewImageFile.ContentType;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio image {ImageId} updated for tailor {TailorId}", imageId, tailorId);

                return (true, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio image {ImageId}", imageId);
            return (false, "An error occurred while updating the image");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePortfolioImageAsync(Guid imageId, Guid tailorId)
    {
        try
        {
            var image = await _unitOfWork.Context.PortfolioImages
                .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && p.TailorId == tailorId && !p.IsDeleted);

            if (image == null)
            {
                return (false, "Image not found");
            }

            // Try delete the stored file if exists
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                try
                {
                    await _attachmentService.Delete(image.ImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file for image {ImageId}", imageId);
                }
            }

            // Soft delete
            image.IsDeleted = true;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Portfolio image {ImageId} deleted for tailor {TailorId}", imageId, tailorId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portfolio image {ImageId}", imageId);
            return (false, "An error occurred while deleting the image");
        }
    }

    public async Task<(bool Success, bool IsFeatured, string? ErrorMessage)> ToggleFeaturedAsync(
        Guid imageId,
        Guid tailorId)
    {
        try
        {
            var image = await _unitOfWork.Context.PortfolioImages
                .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && p.TailorId == tailorId && !p.IsDeleted);

            if (image == null)
            {
                return (false, false, "Image not found");
            }

            image.IsFeatured = !image.IsFeatured;

            await _unitOfWork.SaveChangesAsync();

            return (true, image.IsFeatured, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured status for image {ImageId}", imageId);
            return (false, false, "An error occurred");
        }
    }

    public async Task<(byte[]? ImageData, string? ContentType)> GetPortfolioImageDataAsync(Guid imageId)
    {
        try
        {
            var image = await _unitOfWork.Context.PortfolioImages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && !p.IsDeleted);

            if (image == null)
            {
                return (null, null);
            }

            if (image.ImageData != null)
            {
                return (image.ImageData, image.ContentType ?? "image/jpeg");
            }

            // If ImageUrl is present, try to read file from attachments folder
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                // image.ImageUrl expected like "Attachments/gallery/filename.jpg"
                var relative = image.ImageUrl.Replace("Attachments/", string.Empty, StringComparison.OrdinalIgnoreCase).TrimStart('/', '\\');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", relative);
                if (File.Exists(fullPath))
                {
                    var bytes = await File.ReadAllBytesAsync(fullPath);
                    var contentType = image.ContentType ?? "image/jpeg";
                    return (bytes, contentType);
                }
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portfolio image {ImageId}", imageId);
            return (null, null);
        }
    }

    public async Task<IEnumerable<PortfolioImage>> GetPublicPortfolioAsync(Guid tailorId)
    {
        try
        {
            return await _unitOfWork.Context.PortfolioImages
                .Where(p => p.TailorId == tailorId && !p.IsDeleted)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public portfolio for tailor {TailorId}", tailorId);
            return Enumerable.Empty<PortfolioImage>();
        }
    }
}
