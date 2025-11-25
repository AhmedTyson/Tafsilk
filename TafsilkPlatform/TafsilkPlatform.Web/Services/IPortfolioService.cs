using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.TailorManagement;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service interface for tailor portfolio management operations
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Get all portfolio images for a tailor
    /// </summary>
    Task<ManagePortfolioViewModel?> GetTailorPortfolioAsync(Guid tailorId);

    /// <summary>
    /// Add a new portfolio image
    /// </summary>
    Task<(bool Success, Guid? ImageId, string? ErrorMessage)> AddPortfolioImageAsync(
        Guid tailorId, 
        AddPortfolioImageViewModel model);

    /// <summary>
    /// Get portfolio image for editing
    /// </summary>
    Task<EditPortfolioImageViewModel?> GetPortfolioImageForEditAsync(Guid imageId, Guid tailorId);

    /// <summary>
    /// Update an existing portfolio image
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> UpdatePortfolioImageAsync(
        Guid imageId, 
        Guid tailorId, 
        EditPortfolioImageViewModel model);

    /// <summary>
    /// Delete (soft delete) a portfolio image
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeletePortfolioImageAsync(Guid imageId, Guid tailorId);

    /// <summary>
    /// Toggle featured status of a portfolio image
    /// </summary>
    Task<(bool Success, bool IsFeatured, string? ErrorMessage)> ToggleFeaturedAsync(
        Guid imageId, 
        Guid tailorId);

    /// <summary>
    /// Get portfolio image data
    /// </summary>
    Task<(byte[]? ImageData, string? ContentType)> GetPortfolioImageDataAsync(Guid imageId);

    /// <summary>
    /// Get public portfolio for customer viewing
    /// </summary>
    Task<IEnumerable<PortfolioImage>> GetPublicPortfolioAsync(Guid tailorId);
}
