using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Services
{
    public interface IUserService
    {
        Task<bool> UpdateProfilePictureAsync(Guid userId, IFormFile profilePicture);
        Task<bool> RemoveProfilePictureAsync(Guid userId);
  }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _environment;
     private readonly ILogger<UserService> _logger;

        public UserService(
   AppDbContext db,
         IWebHostEnvironment environment,
            ILogger<UserService> logger)
        {
            _db = db;
            _environment = environment;
 _logger = logger;
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

   // Update database WITH tracking
       var user = await _db.Users
     .Include(u => u.Role)
  .FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) return false;

     var role = user.Role?.Name?.ToLower();
switch (role)
       {
   case "customer":
    var customer = await _db.CustomerProfiles
  .FirstOrDefaultAsync(c => c.UserId == userId);
    if (customer != null)
        {
    customer.ProfilePictureData = imageData;
        customer.ProfilePictureContentType = contentType;
    customer.UpdatedAt = DateTime.UtcNow;
   }
    break;

       case "tailor":
         var tailor = await _db.TailorProfiles
 .FirstOrDefaultAsync(t => t.UserId == userId);
     if (tailor != null)
   {
      tailor.ProfilePictureData = imageData;
        tailor.ProfilePictureContentType = contentType;
     tailor.UpdatedAt = DateTime.UtcNow;
             }
        break;

      case "corporate":
     var corporate = await _db.CorporateAccounts
     .FirstOrDefaultAsync(c => c.UserId == userId);
   if (corporate != null)
 {
    corporate.ProfilePictureData = imageData;
        corporate.ProfilePictureContentType = contentType;
      corporate.UpdatedAt = DateTime.UtcNow;
   }
 break;
          }

// Save changes
    await _db.SaveChangesAsync();

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
       var user = await _db.Users
       .Include(u => u.Role)
         .FirstOrDefaultAsync(u => u.Id == userId);
      if (user == null) return false;

   var role = user.Role?.Name?.ToLower();

     switch (role)
 {
  case "customer":
     var customer = await _db.CustomerProfiles
  .FirstOrDefaultAsync(c => c.UserId == userId);
  if (customer != null)
            {
    customer.ProfilePictureData = null;
      customer.ProfilePictureContentType = null;
          customer.UpdatedAt = DateTime.UtcNow;
     }
 break;

     case "tailor":
      var tailor = await _db.TailorProfiles
    .FirstOrDefaultAsync(t => t.UserId == userId);
          if (tailor != null)
   {
       tailor.ProfilePictureData = null;
       tailor.ProfilePictureContentType = null;
   tailor.UpdatedAt = DateTime.UtcNow;
 }
      break;

         case "corporate":
     var corporate = await _db.CorporateAccounts
  .FirstOrDefaultAsync(c => c.UserId == userId);
         if (corporate != null)
   {
         corporate.ProfilePictureData = null;
    corporate.ProfilePictureContentType = null;
corporate.UpdatedAt = DateTime.UtcNow;
}
  break;
      }

      // Save changes
    await _db.SaveChangesAsync();

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
