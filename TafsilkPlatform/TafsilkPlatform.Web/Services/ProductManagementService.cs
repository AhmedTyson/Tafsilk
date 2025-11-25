using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for tailor product management operations
/// </summary>
public class ProductManagementService : IProductManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ProductManagementService> _logger;

    public ProductManagementService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        ILogger<ProductManagementService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManageProductsViewModel?> GetTailorProductsAsync(Guid tailorId)
    {
        try
        {
            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
            {
                _logger.LogWarning("Tailor {TailorId} not found", tailorId);
                return null;
            }

            // Use UnitOfWork context for complex queries
            var products = await _unitOfWork.Context.Products
                .Where(p => p.TailorId == tailorId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return new ManageProductsViewModel
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for tailor {TailorId}", tailorId);
            return null;
        }
    }

    public async Task<(bool Success, Guid? ProductId, string? ErrorMessage)> AddProductAsync(
        Guid tailorId,
        AddProductViewModel model)
    {
        try
        {
            _logger.LogInformation("[ProductManagement] Starting AddProductAsync for tailor {TailorId}", tailorId);

            // Validate tailor exists
            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
            {
                _logger.LogWarning("[ProductManagement] Tailor {TailorId} not found", tailorId);
                return (false, null, "الملف الشخصي غير موجود");
            }

            _logger.LogInformation("[ProductManagement] Tailor {TailorId} found: {TailorName}", tailorId, tailor.FullName);

            // Validate primary image
            if (model.PrimaryImage == null || model.PrimaryImage.Length == 0)
            {
                _logger.LogWarning("[ProductManagement] Primary image missing for tailor {TailorId}", tailorId);
                return (false, null, "الصورة الأساسية مطلوبة");
            }

            _logger.LogInformation("[ProductManagement] Primary image received: {FileName}, Size: {Size} bytes",
                model.PrimaryImage.FileName, model.PrimaryImage.Length);

            if (!_fileUploadService.IsValidImage(model.PrimaryImage))
            {
                _logger.LogWarning("[ProductManagement] Invalid image type for tailor {TailorId}: {ContentType}",
                    tailorId, model.PrimaryImage.ContentType);
                return (false, null, "نوع الصورة غير صالح. يرجى اختيار صورة (JPG, PNG, GIF, WEBP)");
            }

            if (model.PrimaryImage.Length > _fileUploadService.GetMaxFileSizeInBytes())
            {
                _logger.LogWarning("[ProductManagement] Image too large for tailor {TailorId}: {Size} bytes",
                    tailorId, model.PrimaryImage.Length);
                return (false, null, $"حجم الصورة كبير جداً. الحد الأقصى {_fileUploadService.GetMaxFileSizeInBytes() / 1024 / 1024} ميجابايت");
            }

            // Read primary image data
            byte[] primaryImageData;
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    _logger.LogDebug("[ProductManagement] Copying primary image to memory stream (expected size: {Size} bytes)", model.PrimaryImage.Length);
                    await model.PrimaryImage.CopyToAsync(memoryStream);
                    primaryImageData = memoryStream.ToArray();
                }

                _logger.LogInformation("[ProductManagement] Primary image copied successfully, actual size: {Size} bytes", primaryImageData.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductManagement] Failed to read primary image stream for tailor {TailorId}", tailorId);
                return (false, null, "فشل قراءة الصورة. يرجى المحاولة بملف آخر أو بحجم أصغر.");
            }

            // Process additional images
            string? additionalImagesJson = null;
            if (model.AdditionalImages != null && model.AdditionalImages.Any())
            {
                _logger.LogInformation("[ProductManagement] Processing {Count} additional images", model.AdditionalImages.Count);

                var additionalImagesBase64 = new List<string>();
                foreach (var image in model.AdditionalImages.Take(5)) // Max 5 additional images
                {
                    if (_fileUploadService.IsValidImage(image))
                    {
                        try
                        {
                            using var ms = new MemoryStream();
                            await image.CopyToAsync(ms);
                            var base64 = Convert.ToBase64String(ms.ToArray());
                            additionalImagesBase64.Add($"{image.ContentType}|{base64}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[ProductManagement] Failed to process additional image: {FileName}", image.FileName);
                        }
                    }
                }

                if (additionalImagesBase64.Any())
                {
                    additionalImagesJson = string.Join(";;", additionalImagesBase64);
                }

                _logger.LogInformation("[ProductManagement] Processed {Count} additional images successfully", additionalImagesBase64.Count);
            }

            // Generate slug from name
            var slug = GenerateSlug(model.Name);

            // Ensure unique slug
            var existingSlug = await _unitOfWork.Context.Products
                .AnyAsync(p => p.Slug == slug && !p.IsDeleted);
            if (existingSlug)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                _logger.LogInformation("[ProductManagement] Slug already exists, using unique slug: {Slug}", slug);
            }

            var productId = Guid.NewGuid();

            _logger.LogInformation("[ProductManagement] Creating product entity with ID {ProductId}", productId);

            // Create product with ALL required fields
            var product = new Product
            {
                ProductId = productId,
                Name = model.Name?.Trim() ?? throw new ArgumentException("اسم المنتج مطلوب"),
                Description = model.Description?.Trim() ?? throw new ArgumentException("وصف المنتج مطلوب"),
                Price = model.Price,
                DiscountedPrice = model.DiscountedPrice,
                Category = model.Category ?? throw new ArgumentException("التصنيف مطلوب"),
                SubCategory = model.SubCategory?.Trim(),
                Size = model.Size?.Trim(),
                Color = model.Color?.Trim(),
                Material = model.Material?.Trim(),
                Brand = model.Brand?.Trim(),
                StockQuantity = model.StockQuantity >= 0 ? model.StockQuantity : 0,
                IsAvailable = model.IsAvailable,
                IsFeatured = model.IsFeatured,
                PrimaryImageData = primaryImageData,
                PrimaryImageContentType = model.PrimaryImage.ContentType ?? "image/jpeg",
                AdditionalImagesJson = additionalImagesJson,
                MetaTitle = !string.IsNullOrWhiteSpace(model.MetaTitle)
                    ? model.MetaTitle.Trim()
                    : model.Name.Trim(),
                MetaDescription = !string.IsNullOrWhiteSpace(model.MetaDescription)
                    ? model.MetaDescription.Trim()
                    : (model.Description.Length > 160
                        ? model.Description.Substring(0, 160).Trim()
                        : model.Description.Trim()),
                Slug = slug,
                TailorId = tailorId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null,
                IsDeleted = false,
                ViewCount = 0,
                SalesCount = 0,
                AverageRating = 0.0,
                ReviewCount = 0
            };

            _logger.LogInformation(
                "[ProductManagement] Product entity created. ProductId: {ProductId}, Name: {Name}, TailorId: {TailorId}, Price: {Price}, Stock: {Stock}",
                product.ProductId, product.Name, product.TailorId, product.Price, product.StockQuantity);

            // Use UnitOfWork transaction management
            _logger.LogInformation("[ProductManagement] Starting transaction for product {ProductId}", productId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("[ProductManagement] Adding product {ProductId} to repository", productId);
                await _unitOfWork.Products.AddAsync(product);

                _logger.LogInformation("[ProductManagement] Calling SaveChangesAsync for product {ProductId}", productId);

                var saveResult = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProductManagement] SaveChangesAsync returned {Result} for product {ProductId}", saveResult, productId);

                if (saveResult == 0)
                {
                    _logger.LogError("[ProductManagement] SaveChangesAsync returned 0 - no rows affected for product {ProductId}", productId);
                    throw new InvalidOperationException("فشل حفظ المنتج في قاعدة البيانات");
                }

                // Verify the product was saved
                _logger.LogInformation("[ProductManagement] Verifying product {ProductId} was saved", productId);
                var savedProduct = await _unitOfWork.Context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && !p.IsDeleted);

                if (savedProduct == null)
                {
                    _logger.LogError("[ProductManagement] Product {ProductId} not found after save", productId);
                    throw new InvalidOperationException("فشل التحقق من حفظ المنتج");
                }

                _logger.LogInformation(
                    "[ProductManagement] ✅ Product {ProductId} created successfully. Name: {Name}, Price: {Price}",
                    product.ProductId, savedProduct.Name, savedProduct.Price);

                return (true, product.ProductId, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductManagement] ❌ Error adding product for tailor {TailorId}. Exception: {Exception}", tailorId, ex.ToString());
            return (false, null, $"حدث خطأ: {ex.Message}");
        }
    }

    public async Task<EditProductViewModel?> GetProductForEditAsync(Guid productId, Guid tailorId)
    {
        try
        {
            var product = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

            if (product == null)
            {
                return null;
            }

            var additionalImagesCount = 0;
            if (!string.IsNullOrEmpty(product.AdditionalImagesJson))
            {
                additionalImagesCount = product.AdditionalImagesJson.Split(";;").Length;
            }

            return new EditProductViewModel
            {
                ProductId = product.ProductId,
                TailorId = tailorId,
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId} for edit", productId);
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(
        Guid productId,
        Guid tailorId,
        EditProductViewModel model)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var product = await _unitOfWork.Context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

                if (product == null)
                {
                    return (false, "المنتج غير موجود");
                }

                // Update product info
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.DiscountedPrice = model.DiscountedPrice;
                product.Category = model.Category;
                product.SubCategory = model.SubCategory;
                product.Size = model.Size;
                product.Color = model.Color;
                product.Material = model.Material;
                product.Brand = model.Brand;
                product.StockQuantity = model.StockQuantity;
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
                        return (false, "نوع الملف غير صالح");
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
                        if (existingImages.Count >= 5) break;

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

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated by tailor {TailorId}", productId, tailorId);

                return (true, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return (false, "حدث خطأ أثناء تحديث المنتج");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(Guid productId, Guid tailorId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var product = await _unitOfWork.Context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

                if (product == null)
                {
                    return (false, "المنتج غير موجود");
                }

                // Check if product has active orders
                var hasActiveOrders = await _unitOfWork.Context.OrderItems
                    .AnyAsync(oi => oi.ProductId == productId &&
                        oi.Order.Status != OrderStatus.Cancelled &&
                        oi.Order.Status != OrderStatus.Delivered);

                if (hasActiveOrders)
                {
                    return (false, "لا يمكن حذف المنتج لأنه مرتبط بطلبات نشطة");
                }

                // Soft delete
                product.IsDeleted = true;
                product.IsAvailable = false;
                product.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} deleted by tailor {TailorId}", productId, tailorId);

                return (true, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return (false, "حدث خطأ أثناء حذف المنتج");
        }
    }

    public async Task<(bool Success, bool IsAvailable, string? ErrorMessage)> ToggleProductAvailabilityAsync(
        Guid productId,
        Guid tailorId)
    {
        try
        {
            var product = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

            if (product == null)
            {
                return (false, false, "المنتج غير موجود");
            }

            product.IsAvailable = !product.IsAvailable;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return (true, product.IsAvailable, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling availability for product {ProductId}", productId);
            return (false, false, "حدث خطأ");
        }
    }

    public async Task<(bool Success, int NewStock, bool IsAvailable, string? ErrorMessage)> UpdateStockAsync(
        Guid productId,
        Guid tailorId,
        int newStock)
    {
        try
        {
            if (newStock < 0 || newStock > 10000)
            {
                return (false, 0, false, "الكمية غير صالحة");
            }

            var product = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

            if (product == null)
            {
                return (false, 0, false, "المنتج غير موجود");
            }

            product.StockQuantity = newStock;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Auto-update availability based on stock
            if (newStock == 0 && product.IsAvailable)
            {
                product.IsAvailable = false;
            }

            await _unitOfWork.SaveChangesAsync();

            return (true, product.StockQuantity, product.IsAvailable, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
            return (false, 0, false, "حدث خطأ");
        }
    }

    public async Task<(byte[]? ImageData, string? ContentType)> GetProductImageAsync(Guid productId)
    {
        try
        {
            var product = await _unitOfWork.Context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null || product.PrimaryImageData == null)
            {
                return (null, null);
            }

            return (product.PrimaryImageData, product.PrimaryImageContentType ?? "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image for product {ProductId}", productId);
            return (null, null);
        }
    }

    /// <summary>
    /// Read stream from IFormFile into byte array safely
    /// Exposed internal for unit testing
    /// </summary>
    internal async Task<byte[]?> ReadFormFileAsBytesAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading IFormFile into memory");
            return null;
        }
    }

    private string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Guid.NewGuid().ToString("N").Substring(0, 8);

        text = text.Trim().ToLowerInvariant();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\u0600-\u06FF]+", "-");
        text = text.Trim('-');

        if (text.Length > 50)
            text = text.Substring(0, 50).TrimEnd('-');

        return string.IsNullOrEmpty(text) ? Guid.NewGuid().ToString("N").Substring(0, 8) : text;
    }
}
