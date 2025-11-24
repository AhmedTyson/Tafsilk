using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Web.Services.Base;
using TafsilkPlatform.Web.Common;

namespace TafsilkPlatform.Web.Services
{
    public class AdminService : BaseService, IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AdminService(
            IUnitOfWork unitOfWork,
            ILogger<AdminService> logger,
            IHttpContextAccessor? httpContextAccessor = null) : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _httpContextAccessor = httpContextAccessor;
        }

        // ==================== PRIVATE HELPER METHODS ====================

        /// <summary>
        /// Logs an admin action to the activity log
        /// </summary>
        private async Task LogAdminActionAsync(Guid adminUserId, string action, string details, string entityType = "System")
        {
            Logger.LogInformation("[AdminAction] Admin {AdminId} performed {Action} on {EntityType}. Details: {Details}",
                adminUserId, action, entityType, details);

            await Task.CompletedTask;
        }

        // ==================== TAILOR VERIFICATION ====================

        /// <summary>
        /// Approves tailor verification.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> VerifyTailorAsync(Guid tailorId, Guid adminId)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(tailorId, nameof(tailorId));
                ValidateGuid(adminId, nameof(adminId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
                    if (tailor == null)
                    {
                        throw new InvalidOperationException("الخياط غير موجود");
                    }

                    tailor.IsVerified = true;
                    tailor.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();
                    await LogAdminActionAsync(adminId, "VerifyTailor", $"تم التحقق من الخياط {tailorId}", "Tailor");

                    return true;
                });
            }, "VerifyTailor", adminId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        /// <summary>
        /// Rejects tailor verification.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RejectTailorAsync(
            Guid tailorId,
            Guid adminId,
            string? reason = null)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(tailorId, nameof(tailorId));
                ValidateGuid(adminId, nameof(adminId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
                    if (tailor == null)
                    {
                        throw new InvalidOperationException("الخياط غير موجود");
                    }

                    tailor.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                    await LogAdminActionAsync(adminId, "RejectTailor", $"تم رفض تحقق الخياط {tailorId}. السبب: {reason}", "Tailor");

                    return true;
                });
            }, "RejectTailor", adminId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        // ==================== USER MANAGEMENT ====================

        /// <summary>
        /// Suspends user account.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> SuspendUserAsync(Guid userId, string? reason = null)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(userId, nameof(userId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null)
                    {
                        throw new InvalidOperationException("المستخدم غير موجود");
                    }

                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();
                    await LogAdminActionAsync(userId, "SuspendUser", $"تم إيقاف حساب المستخدم {userId}. السبب: {reason}", "User");

                    return true;
                });
            }, "SuspendUser", userId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        /// <summary>
        /// Activates suspended user account.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> ActivateUserAsync(Guid userId)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(userId, nameof(userId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null)
                    {
                        throw new InvalidOperationException("المستخدم غير موجود");
                    }

                    user.IsActive = true;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();
                    await LogAdminActionAsync(userId, "ActivateUser", $"تم تفعيل حساب المستخدم {userId}", "User");

                    return true;
                });
            }, "ActivateUser", userId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        /// <summary>
        /// Permanently bans user account.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> BanUserAsync(Guid userId, string? reason = null)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(userId, nameof(userId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null)
                    {
                        throw new InvalidOperationException("المستخدم غير موجود");
                    }

                    user.IsActive = false;
                    user.IsDeleted = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.BannedAt = DateTime.UtcNow;
                    user.BanReason = reason ?? "تم حظر الحساب من قبل الإدارة";
                    user.BanExpiresAt = null; // Permanent ban

                    await _unitOfWork.SaveChangesAsync();
                    await LogAdminActionAsync(userId, "BanUser", $"تم حظر الحساب {userId} نهائياً. السبب: {reason}", "User");

                    return true;
                });
            }, "BanUser", userId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        /// <summary>
        /// Deletes user account (soft delete).
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId, string? reason = null)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(userId, nameof(userId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null)
                    {
                        throw new InvalidOperationException("المستخدم غير موجود");
                    }

                    user.IsDeleted = true;
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();

                    return true;
                });
            }, "DeleteUser", userId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        // ==================== PORTFOLIO REVIEW ====================

        /// <summary>
        /// Approves portfolio image.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> ApprovePortfolioImageAsync(Guid imageId, Guid adminId)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(imageId, nameof(imageId));
                ValidateGuid(adminId, nameof(adminId));

                var image = await _unitOfWork.PortfolioImages.GetByIdAsync(imageId);
                if (image == null)
                {
                    throw new InvalidOperationException("الصورة غير موجودة");
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }, "ApprovePortfolioImage", adminId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }

        /// <summary>
        /// Rejects portfolio image (soft delete).
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RejectPortfolioImageAsync(
            Guid imageId,
            Guid adminId,
            string? reason = null)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(imageId, nameof(imageId));
                ValidateGuid(adminId, nameof(adminId));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var image = await _unitOfWork.PortfolioImages.GetByIdAsync(imageId);
                    if (image == null)
                    {
                        throw new InvalidOperationException("الصورة غير موجودة");
                    }

                    image.IsDeleted = true;
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                });
            }, "RejectPortfolioImage", adminId);

            return result.IsSuccess ? (true, null) : (false, result.Error);
        }
    }
}
