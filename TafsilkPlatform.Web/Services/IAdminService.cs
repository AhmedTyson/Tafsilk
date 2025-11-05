namespace TafsilkPlatform.Web.Services
{
    /// <summary>
    /// Service for admin operations like user management and tailor verification.
    /// </summary>
    public interface IAdminService
    {
        // Tailor Verification
        Task<(bool Success, string? ErrorMessage)> VerifyTailorAsync(Guid tailorId, Guid adminId);
        Task<(bool Success, string? ErrorMessage)> RejectTailorAsync(Guid tailorId, Guid adminId, string? reason = null);

        // User Management
        Task<(bool Success, string? ErrorMessage)> SuspendUserAsync(Guid userId, string? reason = null);
        Task<(bool Success, string? ErrorMessage)> ActivateUserAsync(Guid userId);
        Task<(bool Success, string? ErrorMessage)> BanUserAsync(Guid userId, string? reason = null);
        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId, string? reason = null);

        // Portfolio Review
        Task<(bool Success, string? ErrorMessage)> ApprovePortfolioImageAsync(Guid imageId, Guid adminId);
        Task<(bool Success, string? ErrorMessage)> RejectPortfolioImageAsync(Guid imageId, Guid adminId, string? reason = null);
    }
}
