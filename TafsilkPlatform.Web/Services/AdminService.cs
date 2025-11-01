using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
        IUnitOfWork unitOfWork,
            ILogger<AdminService> logger)
        {
            _unitOfWork = unitOfWork;
   _logger = logger;
        }

     // ==================== PRIVATE HELPER METHODS ====================

        /// <summary>
        /// Creates and sends a notification to a user.
        /// </summary>
        private async Task SendNotificationAsync(
            Guid userId, 
 string title, 
            string message, 
            string type)
        {
     var notification = new Notification
     {
    UserId = userId,
                Title = title,
         Message = message,
   Type = type,
         SentAt = DateTime.UtcNow
    };
  
        await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        // ==================== TAILOR VERIFICATION ====================

  /// <summary>
        /// Approves tailor verification.
 /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> VerifyTailorAsync(
   Guid tailorId,
  Guid adminId)
        {
            try
{
    _logger.LogInformation("[AdminService] Verifying tailor: {TailorId} by admin: {AdminId}", tailorId, adminId);

                var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
      if (tailor == null)
  {
         return (false, "الخياط غير موجود");
       }

          tailor.IsVerified = true;
tailor.UpdatedAt = DateTime.UtcNow;

     await _unitOfWork.SaveChangesAsync();

                // Create notification for tailor using helper method
         await SendNotificationAsync(
         tailor.UserId,
          "تم التحقق من حسابك",
   "تهانينا! تم التحقق من حسابك بنجاح. يمكنك الآن استقبال الطلبات.",
 "Success"
      );

  _logger.LogInformation("[AdminService] Tailor verified successfully");
         return (true, null);
    }
catch (Exception ex)
            {
        _logger.LogError(ex, "[AdminService] Error verifying tailor");
       return (false, $"حدث خطأ: {ex.Message}");
     }
 }

      /// <summary>
        /// Rejects tailor verification.
      /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RejectTailorAsync(
     Guid tailorId,
 Guid adminId,
            string? reason = null)
        {
       try
   {
      _logger.LogInformation("[AdminService] Rejecting tailor: {TailorId} by admin: {AdminId}", tailorId, adminId);

         var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
        if (tailor == null)
  {
              return (false, "الخياط غير موجود");
     }

             tailor.UpdatedAt = DateTime.UtcNow;
   await _unitOfWork.SaveChangesAsync();

      // Create notification for tailor using helper method
                await SendNotificationAsync(
        tailor.UserId,
       "تم رفض طلب التحقق",
  $"عذراً، تم رفض طلب التحقق من حسابك. {(string.IsNullOrEmpty(reason) ? "" : $"السبب: {reason}")}",
 "Warning"
  );

      _logger.LogInformation("[AdminService] Tailor rejected");
       return (true, null);
  }
   catch (Exception ex)
            {
       _logger.LogError(ex, "[AdminService] Error rejecting tailor");
         return (false, $"حدث خطأ: {ex.Message}");
}
        }

        // ==================== USER MANAGEMENT ====================

        /// <summary>
 /// Suspends user account.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> SuspendUserAsync(
   Guid userId,
        string? reason = null)
        {
      try
         {
   _logger.LogInformation("[AdminService] Suspending user: {UserId}", userId);

   var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null)
   {
             return (false, "المستخدم غير موجود");
     }

    user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

    await _unitOfWork.SaveChangesAsync();

 // Create notification using helper method
                await SendNotificationAsync(
       userId,
     "تم إيقاف حسابك مؤقتاً",
       $"تم إيقاف حسابك مؤقتاً. {(string.IsNullOrEmpty(reason) ? "" : $"السبب: {reason}")}",
        "Warning"
     );

                _logger.LogInformation("[AdminService] User suspended successfully");
     return (true, null);
            }
      catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminService] Error suspending user");
    return (false, $"حدث خطأ: {ex.Message}");
      }
    }

        /// <summary>
        /// Activates suspended user account.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> ActivateUserAsync(Guid userId)
        {
            try
    {
             _logger.LogInformation("[AdminService] Activating user: {UserId}", userId);

      var user = await _unitOfWork.Users.GetByIdAsync(userId);
   if (user == null)
  {
        return (false, "المستخدم غير موجود");
              }

                user.IsActive = true;
         user.UpdatedAt = DateTime.UtcNow;

           await _unitOfWork.SaveChangesAsync();

                // Create notification using helper method
     await SendNotificationAsync(
            userId,
     "تم تفعيل حسابك",
         "تم تفعيل حسابك بنجاح. يمكنك الآن الدخول واستخدام المنصة.",
         "Success"
     );

       _logger.LogInformation("[AdminService] User activated successfully");
  return (true, null);
      }
            catch (Exception ex)
            {
     _logger.LogError(ex, "[AdminService] Error activating user");
         return (false, $"حدث خطأ: {ex.Message}");
  }
        }

        /// <summary>
    /// Permanently bans user account.
      /// </summary>
     public async Task<(bool Success, string? ErrorMessage)> BanUserAsync(
 Guid userId,
        string? reason = null)
 {
try
      {
          _logger.LogInformation("[AdminService] Banning user: {UserId}", userId);

         var user = await _unitOfWork.Users.GetByIdAsync(userId);
 if (user == null)
       {
          return (false, "المستخدم غير موجود");
      }

  user.IsActive = false;
       user.IsDeleted = true; // Mark as deleted/banned
      user.UpdatedAt = DateTime.UtcNow;

      await _unitOfWork.SaveChangesAsync();

             // Create banned user record
    var bannedUser = new BannedUser
        {
     UserId = userId,
    Reason = reason ?? "تم حظر الحساب من قبل الإدارة",
              BannedAt = DateTime.UtcNow
     };
await _unitOfWork.Context.BannedUsers.AddAsync(bannedUser);

          // Create notification using helper method
          await SendNotificationAsync(
    userId,
                "تم حظر حسابك",
   $"تم حظر حسابك نهائياً. {(string.IsNullOrEmpty(reason) ? "" : $"السبب: {reason}")}",
 "Error"
        );

  _logger.LogInformation("[AdminService] User banned successfully");
  return (true, null);
        }
       catch (Exception ex)
    {
       _logger.LogError(ex, "[AdminService] Error banning user");
                return (false, $"حدث خطأ: {ex.Message}");
     }
   }

        /// <summary>
        /// Deletes user account (soft delete).
  /// </summary>
  public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(
Guid userId,
            string? reason = null)
        {
      try
            {
       _logger.LogInformation("[AdminService] Deleting user: {UserId}", userId);

       var user = await _unitOfWork.Users.GetByIdAsync(userId);
             if (user == null)
        {
     return (false, "المستخدم غير موجود");
    }

          user.IsDeleted = true;
        user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

       await _unitOfWork.SaveChangesAsync();

           _logger.LogInformation("[AdminService] User deleted successfully");
           return (true, null);
            }
       catch (Exception ex)
       {
       _logger.LogError(ex, "[AdminService] Error deleting user");
        return (false, $"حدث خطأ: {ex.Message}");
          }
        }

    // ==================== PORTFOLIO REVIEW ====================

  /// <summary>
        /// Approves portfolio image.
        /// </summary>
      public async Task<(bool Success, string? ErrorMessage)> ApprovePortfolioImageAsync(
          Guid imageId,
    Guid adminId)
 {
   try
   {
       _logger.LogInformation("[AdminService] Approving portfolio image: {ImageId} by admin: {AdminId}", imageId, adminId);

    var image = await _unitOfWork.PortfolioImages.GetByIdAsync(imageId);
  if (image == null)
             {
        return (false, "الصورة غير موجودة");
 }

   // Images are approved by default when uploaded
     // This method can be used to explicitly mark as reviewed

             await _unitOfWork.SaveChangesAsync();

         _logger.LogInformation("[AdminService] Portfolio image approved successfully");
    return (true, null);
    }
            catch (Exception ex)
            {
        _logger.LogError(ex, "[AdminService] Error approving portfolio image");
     return (false, $"حدث خطأ: {ex.Message}");
     }
        }

        /// <summary>
  /// Rejects portfolio image (soft delete).
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RejectPortfolioImageAsync(
 Guid imageId,
            Guid adminId,
      string? reason = null)
        {
   try
    {
      _logger.LogInformation("[AdminService] Rejecting portfolio image: {ImageId} by admin: {AdminId}", imageId, adminId);

    var image = await _unitOfWork.PortfolioImages.GetByIdAsync(imageId);
                if (image == null)
              {
return (false, "الصورة غير موجودة");
          }

      image.IsDeleted = true;
     await _unitOfWork.SaveChangesAsync();

         // Notify tailor using helper method
        var tailor = await _unitOfWork.Tailors.GetByIdAsync(image.TailorId);
  if (tailor != null)
                {
                await SendNotificationAsync(
        tailor.UserId,
 "تم رفض صورة من معرض أعمالك",
                $"تم رفض إحدى الصور من معرض أعمالك. {(string.IsNullOrEmpty(reason) ? "" : $"السبب: {reason}")}",
        "Warning"
                 );
                }

     _logger.LogInformation("[AdminService] Portfolio image rejected successfully");
     return (true, null);
            }
            catch (Exception ex)
      {
          _logger.LogError(ex, "[AdminService] Error rejecting portfolio image");
    return (false, $"حدث خطأ: {ex.Message}");
    }
        }
    }
}
