using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.TailorManagement;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for tailor product management operations
/// </summary>
public class ProductManagementService : IProductManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductManagementService> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAttachmentService _attachmentService;

    public ProductManagementService(
        IUnitOfWork unitOfWork,
        ILogger<ProductManagementService> logger,
        IFileUploadService fileUploadService,
        IAttachmentService attachmentService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
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
                TailorName = tailor.FullName ?? "Tailor",
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
                    HasImage = p.PrimaryImageData != null || !string.IsNullOrEmpty(p.PrimaryImageUrl)
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
                return (false, null, "Profile not found");
            }

            _logger.LogInformation("[ProductManagement] Tailor {TailorId} found: {TailorName}", tailorId, tailor.FullName);

            // Validate primary image
            if (model.PrimaryImage == null || model.PrimaryImage.Length == 0)
            {
                _logger.LogWarning("[ProductManagement] Primary image missing for tailor {TailorId}", tailorId);
                return (false, null, "Primary image is required");
            }

            _logger.LogInformation("[ProductManagement] Primary image received: {FileName}, Size: {Size} bytes",
                model.PrimaryImage.FileName, model.PrimaryImage.Length);

            if (!await _fileUploadService.IsValidImageAsync(model.PrimaryImage))
            {
                _logger.LogWarning("[ProductManagement] Invalid image type for tailor {TailorId}: {ContentType}",
                    tailorId, model.PrimaryImage.ContentType);
                return (false, null, "Invalid image type. Please choose an image (JPG, PNG, GIF, WEBP)");
            }

            if (model.PrimaryImage.Length > _fileUploadService.GetMaxFileSizeInBytes())
            {
                _logger.LogWarning("[ProductManagement] Image too large for tailor {TailorId}: {Size} bytes",
                    tailorId, model.PrimaryImage.Length);
                return (false, null, $"Image size is too large. Maximum size is {_fileUploadService.GetMaxFileSizeInBytes() / 1024 / 1024} MB");
            }

            // Upload primary image to file system
            var primaryImagePath = await _attachmentService.Upload(model.PrimaryImage, "product");
            if (string.IsNullOrEmpty(primaryImagePath))
            {
                return (false, null, "Failed to upload primary image");
            }

            // Read primary image data (backup/legacy)
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
                return (false, null, "Failed to read image. Please try another file or smaller size.");
            }

            // Process additional images
            string? additionalImagesJson = null;
            if (model.AdditionalImages != null && model.AdditionalImages.Any())
            {
                _logger.LogInformation("[ProductManagement] Processing {Count} additional images", model.AdditionalImages.Count);

                var additionalImagesList = new List<string>();
                foreach (var image in model.AdditionalImages.Take(5)) // Max 5 additional images
                {
                    if (await _fileUploadService.IsValidImageAsync(image))
                    {
                        try
                        {
                            var imgPath = await _attachmentService.Upload(image, "product");
                            if (!string.IsNullOrEmpty(imgPath))
                            {
                                additionalImagesList.Add(imgPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[ProductManagement] Failed to process additional image: {FileName}", image.FileName);
                        }
                    }
                }

                if (additionalImagesList.Any())
                {
                    additionalImagesJson = JsonSerializer.Serialize(additionalImagesList);
                }

                _logger.LogInformation("[ProductManagement] Processed {Count} additional images successfully", additionalImagesList.Count);
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
                Name = model.Name?.Trim() ?? throw new ArgumentException("Product name is required"),
                Description = model.Description?.Trim() ?? throw new ArgumentException("Product description is required"),
                Price = model.Price,
                DiscountedPrice = model.DiscountedPrice,
                Category = model.Category ?? throw new ArgumentException("Category is required"),
                SubCategory = model.SubCategory?.Trim(),
                Size = model.Size?.Trim(),
                Color = model.Color?.Trim(),
                Material = model.Material?.Trim(),
                Brand = model.Brand?.Trim(),
                StockQuantity = model.StockQuantity >= 0 ? model.StockQuantity : 0,
                IsAvailable = model.IsAvailable,
                IsFeatured = model.IsFeatured,
                PrimaryImageUrl = primaryImagePath,
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
                    throw new InvalidOperationException("Failed to save product to database");
                }

                // Verify the product was saved
                _logger.LogInformation("[ProductManagement] Verifying product {ProductId} was saved", productId);
                var savedProduct = await _unitOfWork.Context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && !p.IsDeleted);

                if (savedProduct == null)
                {
                    _logger.LogError("[ProductManagement] Product {ProductId} not found after save", productId);
                    throw new InvalidOperationException("Failed to verify product save");
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
            return (false, null, $"An error occurred: {ex.Message}");
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
                try
                {
                    // Try to parse as JSON array (new format)
                    var images = JsonSerializer.Deserialize<List<string>>(product.AdditionalImagesJson);
                    additionalImagesCount = images?.Count ?? 0;
                }
                catch
                {
                    // Fallback to old format (semicolon separated)
                    additionalImagesCount = product.AdditionalImagesJson.Split(";;").Length;
                }
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
                HasCurrentPrimaryImage = product.PrimaryImageData != null || !string.IsNullOrEmpty(product.PrimaryImageUrl),
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
                    return (false, "Product not found");
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
                    if (!await _fileUploadService.IsValidImageAsync(model.NewPrimaryImage))
                    {
                        return (false, "Invalid file type");
                    }

                    // Delete old image if it exists as a file
                    if (!string.IsNullOrEmpty(product.PrimaryImageUrl))
                    {
                        await _attachmentService.Delete(product.PrimaryImageUrl);
                    }

                    // Upload new image
                    var newImagePath = await _attachmentService.Upload(model.NewPrimaryImage, "product");
                    if (string.IsNullOrEmpty(newImagePath))
                    {
                        return (false, "Failed to upload new image");
                    }

                    product.PrimaryImageUrl = newImagePath;
                    product.PrimaryImageContentType = model.NewPrimaryImage.ContentType;

                    // Update binary data for backward compatibility
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.NewPrimaryImage.CopyToAsync(memoryStream);
                        product.PrimaryImageData = memoryStream.ToArray();
                    }
                }

                // Add new additional images if provided
                if (model.NewAdditionalImages != null && model.NewAdditionalImages.Any())
                {
                    var currentImages = new List<string>();
                    if (!string.IsNullOrEmpty(product.AdditionalImagesJson))
                    {
                        try 
                        {
                            currentImages = JsonSerializer.Deserialize<List<string>>(product.AdditionalImagesJson) ?? new List<string>();
                        }
                        catch 
                        { 
                            // Ignore deserialization errors or handle legacy format if needed
                        }
                    }

                    foreach (var image in model.NewAdditionalImages)
                    {
                        if (currentImages.Count >= 5) break;

                        if (await _fileUploadService.IsValidImageAsync(image))
                        {
                            var imgPath = await _attachmentService.Upload(image, "product");
                            if (!string.IsNullOrEmpty(imgPath))
                            {
                                currentImages.Add(imgPath);
                            }
                        }
                    }

                    product.AdditionalImagesJson = JsonSerializer.Serialize(currentImages);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated by tailor {TailorId}", productId, tailorId);

                return (true, (string?)null);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return (false, "An error occurred while updating the product");
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
                    return (false, "Product not found");
                }

                // Check if product has active orders
                var hasActiveOrders = await _unitOfWork.Context.OrderItems
                    .AnyAsync(oi => oi.ProductId == productId &&
                        oi.Order.Status != OrderStatus.Cancelled &&
                        oi.Order.Status != OrderStatus.Delivered);

                if (hasActiveOrders)
                {
                    return (false, "Cannot delete product because it is linked to active orders");
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
            return (false, "An error occurred while deleting the product");
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
                return (false, false, "Product not found");
            }

            product.IsAvailable = !product.IsAvailable;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return (true, product.IsAvailable, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling availability for product {ProductId}", productId);
            return (false, false, "An error occurred");
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
                return (false, 0, false, "Invalid quantity");
            }

            var product = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.TailorId == tailorId && !p.IsDeleted);

            if (product == null)
            {
                return (false, 0, false, "Product not found");
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
            return (false, 0, false, "An error occurred");
        }
    }

    public async Task<(byte[]? ImageData, string? ContentType)> GetProductImageAsync(Guid productId)
    {
        try
        {
            var product = await _unitOfWork.Context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null)
            {
                return (null, null);
            }

            // Note: This method returns binary data. 
            // If using URLs, the controller should check PrimaryImageUrl first.
            // This is kept for backward compatibility or when binary data is explicitly needed.
            
            if (product.PrimaryImageData == null)
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
