using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Security;

namespace TafsilkPlatform.Web.Services
{
 public interface IUserService
    {
        Task<UserSettingsViewModel?> GetUserSettingsAsync(Guid userId);
        Task<(bool Succeeded, string? Error)> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request);
 Task<bool> UpdateProfilePictureAsync(Guid userId, IFormFile profilePicture);
  Task<bool> RemoveProfilePictureAsync(Guid userId);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
    private readonly ITailorRepository _tailorRepository;
        private readonly ICorporateRepository _corporateRepository;
        private readonly IWebHostEnvironment _environment;
 private readonly ILogger<UserService> _logger;

        public UserService(
   AppDbContext db,
            IUserRepository userRepository,
ICustomerRepository customerRepository,
  ITailorRepository tailorRepository,
  ICorporateRepository corporateRepository,
   IWebHostEnvironment environment,
            ILogger<UserService> logger)
 {
   _db = db;
_userRepository = userRepository;
   _customerRepository = customerRepository;
        _tailorRepository = tailorRepository;
           _corporateRepository = corporateRepository;
  _environment = environment;
 _logger = logger;
        }

        public async Task<UserSettingsViewModel?> GetUserSettingsAsync(Guid userId)
        {
            try
 {
      var user = await _db.Users
             .Include(u => u.Role)
.FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null)
      {
                    _logger.LogWarning("User not found: {UserId}", userId);
         return null;
         }

var settings = new UserSettingsViewModel
   {
               UserId = user.Id,
 Email = user.Email,
      PhoneNumber = user.PhoneNumber,
          EmailNotifications = user.EmailNotifications,
      SmsNotifications = user.SmsNotifications,
           PromotionalNotifications = user.PromotionalNotifications,
   Role = user.Role?.Name ?? "Customer"
 };

    switch (user.Role?.Name?.ToLower())
                {
     case "customer":
       var customer = await _customerRepository.GetByUserIdAsync(userId);
       if (customer != null)
  {
    settings.FullName = customer.FullName ?? string.Empty;
      settings.City = customer.City;
    settings.DateOfBirth = customer.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
  settings.Bio = customer.Bio;
         settings.Gender = customer.Gender;
          // Use database-stored image URL instead of deprecated ProfilePictureUrl
       if (customer.ProfilePictureData != null)
       {
     settings.ProfilePictureUrl = $"/Account/ProfilePicture/{userId}";
   }
           }
  else
           {
settings.FullName = user.Email; // Fallback
      }
   break;

        case "tailor":
         var tailor = await _tailorRepository.GetByUserIdAsync(userId);
            if (tailor != null)
       {
        settings.FullName = tailor.FullName ?? string.Empty;
             settings.ShopName = tailor.ShopName;
                settings.Address = tailor.Address;
    settings.City = tailor.City;
           settings.Bio = tailor.Bio;
             settings.ExperienceYears = tailor.ExperienceYears;
          settings.PricingRange = tailor.PricingRange;
      // Use database-stored image URL
         if (tailor.ProfilePictureData != null)
    {
               settings.ProfilePictureUrl = $"/Account/ProfilePicture/{userId}";
     }
  }
              else
     {
     settings.FullName = user.Email; // Fallback
                    }
  break;

     case "corporate":
       var corporate = await _corporateRepository.GetByUserIdAsync(userId);
  if (corporate != null)
      {
           settings.FullName = corporate.ContactPerson ?? corporate.CompanyName ?? string.Empty;
         settings.CompanyName = corporate.CompanyName;
          settings.ContactPerson = corporate.ContactPerson;
        settings.Bio = corporate.Bio;
          // Use database-stored image URL
   if (corporate.ProfilePictureData != null)
           {
     settings.ProfilePictureUrl = $"/Account/ProfilePicture/{userId}";
     }
              }
               else
    {
        settings.FullName = user.Email; // Fallback
          }
    break;

              default:
   settings.FullName = user.Email; // Fallback for any role
          break;
         }

                return settings;
      }
         catch (Exception ex)
            {
    _logger.LogError(ex, "Error loading user settings for user: {UserId}", userId);
        return null;
            }
        }

  public async Task<(bool Succeeded, string? Error)> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request)
        {
    using var transaction = await _db.Database.BeginTransactionAsync();
       try
          {
  var user = await _db.Users
       .Include(u => u.Role)
           .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
    return (false, "المستخدم غير موجود");

 // Validate email uniqueness
    if (user.Email != request.Email && await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
      return (false, "البريد الإلكتروني مستخدم من قبل");

     // Validate phone number uniqueness
      if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber && await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
           return (false, "رقم الهاتف مستخدم من قبل");

           // Handle password change
      if (!string.IsNullOrEmpty(request.NewPassword))
       {
           if (string.IsNullOrEmpty(request.CurrentPassword))
      return (false, "يجب إدخال كلمة المرور الحالية");
       if (!PasswordHasher.Verify(user.PasswordHash, request.CurrentPassword))
    return (false, "كلمة المرور الحالية غير صحيحة");
   user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
      }

    // Update user base properties
        user.Email = request.Email;
user.PhoneNumber = request.PhoneNumber;
            user.EmailNotifications = request.EmailNotifications;
       user.SmsNotifications = request.SmsNotifications;
        user.PromotionalNotifications = request.PromotionalNotifications;
        user.UpdatedAt = DateTime.UtcNow;
     await _userRepository.UpdateAsync(user);

            // Update role-specific data
       var role = user.Role?.Name?.ToLower();
           switch (role)
       {
     case "customer":
           var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
      {
         customer.FullName = request.FullName;
     customer.City = request.City;
     customer.Gender = request.Gender;
 customer.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null;
    customer.Bio = request.Bio;
  await _customerRepository.UpdateAsync(customer);
}
     break;
          case "tailor":
        var tailor = await _tailorRepository.GetByUserIdAsync(userId);
              if (tailor != null)
             {
      tailor.FullName = request.FullName;
      tailor.ShopName = request.ShopName ?? tailor.ShopName;
  tailor.Address = request.Address ?? tailor.Address;
     tailor.City = request.City;
      tailor.ExperienceYears = request.ExperienceYears;
       tailor.PricingRange = request.PricingRange;
    tailor.Bio = request.Bio;
    await _tailorRepository.UpdateAsync(tailor);
    }
        break;
     case "corporate":
          var corporate = await _corporateRepository.GetByUserIdAsync(userId);
      if (corporate != null)
     {
        corporate.CompanyName = request.CompanyName ?? corporate.CompanyName;
    corporate.ContactPerson = request.ContactPerson ?? corporate.ContactPerson;
  corporate.Bio = request.Bio;
    await _corporateRepository.UpdateAsync(corporate);
           }
     break;
     }

         await transaction.CommitAsync();
         _logger.LogInformation("Successfully updated settings for user: {UserId}", userId);
     return (true, null);
     }
     catch (Exception ex)
            {
      await transaction.RollbackAsync();
      _logger.LogError(ex, "Failed to update settings for user: {UserId}", userId);
     return (false, "حدث خطأ أثناء حفظ التعديلات");
          }
   }

        public async Task<bool> UpdateProfilePictureAsync(Guid userId, IFormFile profilePicture)
   {
    try
            {
       if (profilePicture == null || profilePicture.Length == 0) return false;

  // Validate file
 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
   var extension = Path.GetExtension(profilePicture.FileName).ToLower();
         if (!allowedExtensions.Contains(extension)) return false;
     if (profilePicture.Length > 5 * 1024 * 1024) return false; // 5MB limit

      // Read image data
    byte[] imageData;
         using (var memoryStream = new MemoryStream())
                {
     await profilePicture.CopyToAsync(memoryStream);
       imageData = memoryStream.ToArray();
    }

  var contentType = profilePicture.ContentType;

    // Update database
      var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

  var role = user.Role?.Name?.ToLower();
            switch (role)
                {
      case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
              if (customer != null)
    {
      customer.ProfilePictureData = imageData;
 customer.ProfilePictureContentType = contentType;
          customer.UpdatedAt = DateTime.UtcNow;
    await _customerRepository.UpdateAsync(customer);
      }
       break;
 case "tailor":
              var tailor = await _tailorRepository.GetByUserIdAsync(userId);
   if (tailor != null)
  {
           tailor.ProfilePictureData = imageData;
     tailor.ProfilePictureContentType = contentType;
       tailor.UpdatedAt = DateTime.UtcNow;
         await _tailorRepository.UpdateAsync(tailor);
      }
      break;
             case "corporate":
              var corporate = await _corporateRepository.GetByUserIdAsync(userId);
        if (corporate != null)
   {
 corporate.ProfilePictureData = imageData;
  corporate.ProfilePictureContentType = contentType;
                  corporate.UpdatedAt = DateTime.UtcNow;
   await _corporateRepository.UpdateAsync(corporate);
 }
         break;
    }

      _logger.LogInformation("Profile picture updated for user: {UserId}", userId);
      return true;
   }
 catch (Exception ex)
       {
           _logger.LogError(ex, "Error updating profile picture for user: {UserId}", userId);
        return false;
            }
     }

 public async Task<bool> RemoveProfilePictureAsync(Guid userId)
        {
         try
            {
       var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
          if (user == null) return false;

             var role = user.Role?.Name?.ToLower();

      switch (role)
             {
      case "customer":
          var customer = await _customerRepository.GetByUserIdAsync(userId);
   if (customer != null)
          {
  customer.ProfilePictureData = null;
               customer.ProfilePictureContentType = null;
       customer.UpdatedAt = DateTime.UtcNow;
              await _customerRepository.UpdateAsync(customer);
    }
    break;
     case "tailor":
            var tailor = await _tailorRepository.GetByUserIdAsync(userId);
      if (tailor != null)
     {
  tailor.ProfilePictureData = null;
    tailor.ProfilePictureContentType = null;
       tailor.UpdatedAt = DateTime.UtcNow;
    await _tailorRepository.UpdateAsync(tailor);
           }
   break;
       case "corporate":
      var corporate = await _corporateRepository.GetByUserIdAsync(userId);
       if (corporate != null)
     {
   corporate.ProfilePictureData = null;
  corporate.ProfilePictureContentType = null;
                corporate.UpdatedAt = DateTime.UtcNow;
        await _corporateRepository.UpdateAsync(corporate);
              }
           break;
     }

      _logger.LogInformation("Profile picture removed for user: {UserId}", userId);
    return true;
            }
   catch (Exception ex)
        {
        _logger.LogError(ex, "Error removing profile picture for user: {UserId}", userId);
            return false;
       }
        }
    }
}
