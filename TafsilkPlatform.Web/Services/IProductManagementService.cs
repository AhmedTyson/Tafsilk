using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service interface for tailor product management operations
/// </summary>
public interface IProductManagementService
{
    /// <summary>
    /// Get all products for a specific tailor
    /// </summary>
    Task<ManageProductsViewModel?> GetTailorProductsAsync(Guid tailorId);

    /// <summary>
    /// Add a new product
    /// </summary>
    Task<(bool Success, Guid? ProductId, string? ErrorMessage)> AddProductAsync(
        Guid tailorId, 
        AddProductViewModel model);

    /// <summary>
    /// Get product for editing
    /// </summary>
    Task<EditProductViewModel?> GetProductForEditAsync(Guid productId, Guid tailorId);

    /// <summary>
    /// Update an existing product
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(
        Guid productId, 
        Guid tailorId, 
        EditProductViewModel model);

    /// <summary>
    /// Delete (soft delete) a product
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(Guid productId, Guid tailorId);

    /// <summary>
    /// Toggle product availability
    /// </summary>
    Task<(bool Success, bool IsAvailable, string? ErrorMessage)> ToggleProductAvailabilityAsync(
        Guid productId, 
        Guid tailorId);

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    Task<(bool Success, int NewStock, bool IsAvailable, string? ErrorMessage)> UpdateStockAsync(
        Guid productId, 
        Guid tailorId, 
        int newStock);

    /// <summary>
    /// Get product image
    /// </summary>
    Task<(byte[]? ImageData, string? ContentType)> GetProductImageAsync(Guid productId);
}
