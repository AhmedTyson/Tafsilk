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
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        ILogger<PortfolioService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
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
                return (false, null, "الملف الشخصي غير موجود");
            }

            // Validate image file
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return (false, null, "يرجى اختيار صورة");
            }

            if (!_fileUploadService.IsValidImage(model.ImageFile))
            {
                return (false, null, "نوع الملف غير صالح. يرجى اختيار صورة (JPG, PNG, GIF)");
            }

            if (model.ImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
            {
                return (false, null, $"حجم الصورة يجب أن يكون أقل من {_fileUploadService.GetMaxFileSizeInBytes() / 1024 / 1024} ميجابايت");
            }

            // Check image count limit
            var currentImageCount = await _unitOfWork.Context.PortfolioImages
                .CountAsync(p => p.TailorId == tailorId && !p.IsDeleted);

            if (currentImageCount >= 50)
            {
                return (false, null, "لقد وصلت إلى الحد الأقصى لعدد الصور (50 صورة)");
            }

            // Read image data
            byte[] imageData;
            using (var memoryStream = new MemoryStream())
            {
                await model.ImageFile.CopyToAsync(memoryStream);
                imageData = memoryStream.ToArray();
            }

            // Get next display order
            var maxOrder = await _unitOfWork.Context.PortfolioImages
                .Where(p => p.TailorId == tailorId && !p.IsDeleted)
                .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

            // Create portfolio image
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
                ImageData = imageData,
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
                    throw new InvalidOperationException("فشل حفظ الصورة في قاعدة البيانات");
                }

                // Verify the portfolio image was saved
                var savedImage = await _unitOfWork.Context.PortfolioImages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PortfolioImageId == portfolioImage.PortfolioImageId && !p.IsDeleted);
                
                if (savedImage == null)
                {
                    _logger.LogError("Portfolio image not found after save. ImageId: {ImageId}", portfolioImage.PortfolioImageId);
                    throw new InvalidOperationException("فشل التحقق من حفظ الصورة");
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
            return (false, null, $"حدث خطأ: {ex.Message}");
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
                HasCurrentImage = image.ImageData != null
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
                    return (false, "الصورة غير موجودة");
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
                        return (false, "نوع الملف غير صالح");
                    }

                    if (model.NewImageFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
                    {
                        return (false, "حجم الملف كبير جداً");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.NewImageFile.CopyToAsync(memoryStream);
                        image.ImageData = memoryStream.ToArray();
                        image.ContentType = model.NewImageFile.ContentType;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio image {ImageId} updated for tailor {TailorId}", imageId, tailorId);

                return (true, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio image {ImageId}", imageId);
            return (false, "حدث خطأ أثناء تحديث الصورة");
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
                return (false, "الصورة غير موجودة");
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
            return (false, "حدث خطأ أثناء حذف الصورة");
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
                return (false, false, "الصورة غير موجودة");
            }

            image.IsFeatured = !image.IsFeatured;
            
            await _unitOfWork.SaveChangesAsync();

            return (true, image.IsFeatured, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured status for image {ImageId}", imageId);
            return (false, false, "حدث خطأ");
        }
    }

    public async Task<(byte[]? ImageData, string? ContentType)> GetPortfolioImageDataAsync(Guid imageId)
    {
        try
        {
            var image = await _unitOfWork.Context.PortfolioImages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && !p.IsDeleted);

            if (image == null || image.ImageData == null)
            {
                return (null, null);
            }

            return (image.ImageData, image.ContentType ?? "image/jpeg");
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
