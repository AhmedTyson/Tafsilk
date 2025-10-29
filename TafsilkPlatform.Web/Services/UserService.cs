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
<<<<<<< Updated upstream
=======
 Task<bool> RemoveProfilePictureAsync(Guid userId);
>>>>>>> Stashed changes
 }

 public class UserService : IUserService
 {
 private readonly AppDbContext _db;
 private readonly IUserRepository _userRepository;
 private readonly ICustomerRepository _customerRepository;
 private readonly ITailorRepository _tailorRepository;
 private readonly ICorporateRepository _corporateRepository;
 private readonly IWebHostEnvironment _environment;
<<<<<<< Updated upstream
=======
 private readonly ILogger<UserService> _logger;
>>>>>>> Stashed changes

 public UserService(
 AppDbContext db,
 IUserRepository userRepository,
 ICustomerRepository customerRepository,
 ITailorRepository tailorRepository,
 ICorporateRepository corporateRepository,
<<<<<<< Updated upstream
 IWebHostEnvironment environment)
=======
 IWebHostEnvironment environment,
 ILogger<UserService> logger)
>>>>>>> Stashed changes
 {
 _db = db;
 _userRepository = userRepository;
 _customerRepository = customerRepository;
 _tailorRepository = tailorRepository;
 _corporateRepository = corporateRepository;
 _environment = environment;
<<<<<<< Updated upstream
=======
 _logger = logger;
>>>>>>> Stashed changes
 }

 public async Task<UserSettingsViewModel?> GetUserSettingsAsync(Guid userId)
 {
<<<<<<< Updated upstream
 var user = await _db.Users
 .Include(u => u.Role)
 .FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null) return null;
=======
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
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
 settings.FullName = customer.FullName;
 settings.City = customer.City;
 settings.DateOfBirth = customer.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
 settings.Bio = customer.Bio;
 settings.ProfilePictureUrl = customer.ProfilePictureUrl;
 }
 break;
=======
 settings.FullName = customer.FullName ?? string.Empty;
 settings.City = customer.City;
 settings.DateOfBirth = customer.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
 settings.Bio = customer.Bio;
 settings.Gender = customer.Gender;
#pragma warning disable CS0618 // Type or member is obsolete
 settings.ProfilePictureUrl = customer.ProfilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
 }
 else
 {
 settings.FullName = user.Email; // Fallback
 }
 break;

>>>>>>> Stashed changes
 case "tailor":
 var tailor = await _tailorRepository.GetByUserIdAsync(userId);
 if (tailor != null)
 {
 settings.FullName = tailor.FullName ?? string.Empty;
 settings.ShopName = tailor.ShopName;
 settings.Address = tailor.Address;
 settings.City = tailor.City;
 settings.Bio = tailor.Bio;
<<<<<<< Updated upstream
 settings.ProfilePictureUrl = tailor.ProfilePictureUrl;
 }
 break;
=======
 settings.ExperienceYears = tailor.ExperienceYears;
 settings.PricingRange = tailor.PricingRange;
#pragma warning disable CS0618 // Type or member is obsolete
 settings.ProfilePictureUrl = tailor.ProfilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
 }
 else
 {
 settings.FullName = user.Email; // Fallback
 }
 break;

>>>>>>> Stashed changes
 case "corporate":
 var corporate = await _corporateRepository.GetByUserIdAsync(userId);
 if (corporate != null)
 {
<<<<<<< Updated upstream
 settings.CompanyName = corporate.CompanyName;
 settings.ContactPerson = corporate.ContactPerson;
 settings.Bio = corporate.Bio;
 settings.ProfilePictureUrl = corporate.ProfilePictureUrl;
 }
=======
 settings.FullName = corporate.ContactPerson ?? corporate.CompanyName ?? string.Empty;
 settings.CompanyName = corporate.CompanyName;
 settings.ContactPerson = corporate.ContactPerson;
 settings.Bio = corporate.Bio;
#pragma warning disable CS0618 // Type or member is obsolete
 settings.ProfilePictureUrl = corporate.ProfilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
 }
 else
 {
 settings.FullName = user.Email; // Fallback
 }
 break;

 default:
 settings.FullName = user.Email; // Fallback for any role
>>>>>>> Stashed changes
 break;
 }

 return settings;
 }
<<<<<<< Updated upstream
=======
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error loading user settings for user: {UserId}", userId);
 return null;
 }
 }
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
 if (user.Email != request.Email && await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
 return (false, "البريد الإلكتروني مستخدم من قبل");

 if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber && await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
 return (false, "رقم الهاتف مستخدم من قبل");

=======
 // Validate email uniqueness
 if (user.Email != request.Email && await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
 return (false, "البريد الإلكتروني مستخدم من قبل");

 // Validate phone number uniqueness
 if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber && await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
 return (false, "رقم الهاتف مستخدم من قبل");

 // Handle password change
>>>>>>> Stashed changes
 if (!string.IsNullOrEmpty(request.NewPassword))
 {
 if (string.IsNullOrEmpty(request.CurrentPassword))
 return (false, "يجب إدخال كلمة المرور الحالية");
 if (!PasswordHasher.Verify(user.PasswordHash, request.CurrentPassword))
 return (false, "كلمة المرور الحالية غير صحيحة");
 user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
 }

<<<<<<< Updated upstream
=======
 // Update user base properties
>>>>>>> Stashed changes
 user.Email = request.Email;
 user.PhoneNumber = request.PhoneNumber;
 user.EmailNotifications = request.EmailNotifications;
 user.SmsNotifications = request.SmsNotifications;
 user.PromotionalNotifications = request.PromotionalNotifications;
 user.UpdatedAt = DateTime.UtcNow;
 await _userRepository.UpdateAsync(user);

<<<<<<< Updated upstream
=======
 // Update role-specific data
>>>>>>> Stashed changes
 var role = user.Role?.Name?.ToLower();
 switch (role)
 {
 case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
 {
 customer.FullName = request.FullName;
 customer.City = request.City;
<<<<<<< Updated upstream
=======
 customer.Gender = request.Gender;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
 tailor.ExperienceYears = request.ExperienceYears;
 tailor.PricingRange = request.PricingRange;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
 return (true, null);
 }
 catch
 {
 await transaction.RollbackAsync();
=======
 _logger.LogInformation("Successfully updated settings for user: {UserId}", userId);
 return (true, null);
 }
 catch (Exception ex)
 {
 await transaction.RollbackAsync();
 _logger.LogError(ex, "Failed to update settings for user: {UserId}", userId);
>>>>>>> Stashed changes
 return (false, "حدث خطأ أثناء حفظ التعديلات");
 }
 }

 public async Task<bool> UpdateProfilePictureAsync(Guid userId, IFormFile profilePicture)
 {
 try
 {
<<<<<<< Updated upstream
 if (profilePicture == null || profilePicture.Length ==0) return false;
 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
 var extension = Path.GetExtension(profilePicture.FileName).ToLower();
 if (!allowedExtensions.Contains(extension)) return false;
 if (profilePicture.Length >5 *1024 *1024) return false;

 var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
 if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
 var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
 var filePath = Path.Combine(uploadsPath, fileName);
=======
 if (profilePicture == null || profilePicture.Length == 0) return false;

 // Validate file
 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
 var extension = Path.GetExtension(profilePicture.FileName).ToLower();
 if (!allowedExtensions.Contains(extension)) return false;
 if (profilePicture.Length > 5 * 1024 * 1024) return false; // 5MB limit

 // Ensure upload directory exists
 var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
 if (!Directory.Exists(uploadsPath))
 Directory.CreateDirectory(uploadsPath);

 // Generate unique filename
 var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
 var filePath = Path.Combine(uploadsPath, fileName);

 // Save file
>>>>>>> Stashed changes
 using (var stream = new FileStream(filePath, FileMode.Create))
 {
 await profilePicture.CopyToAsync(stream);
 }
 var profilePictureUrl = $"/uploads/profiles/{fileName}";

<<<<<<< Updated upstream
 var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null) return false;
=======
 // Update database
 var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null) return false;

>>>>>>> Stashed changes
 var role = user.Role?.Name?.ToLower();
 switch (role)
 {
 case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
 {
<<<<<<< Updated upstream
 customer.ProfilePictureUrl = profilePictureUrl;
=======
 // Delete old profile picture if exists
#pragma warning disable CS0618 // Type or member is obsolete
 if (!string.IsNullOrEmpty(customer.ProfilePictureUrl))
 {
 DeleteOldProfilePicture(customer.ProfilePictureUrl);
 }
 customer.ProfilePictureUrl = profilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
>>>>>>> Stashed changes
 await _customerRepository.UpdateAsync(customer);
 }
 break;
 case "tailor":
 var tailor = await _tailorRepository.GetByUserIdAsync(userId);
 if (tailor != null)
 {
<<<<<<< Updated upstream
 tailor.ProfilePictureUrl = profilePictureUrl;
=======
 // Delete old profile picture if exists
#pragma warning disable CS0618 // Type or member is obsolete
 if (!string.IsNullOrEmpty(tailor.ProfilePictureUrl))
 {
 DeleteOldProfilePicture(tailor.ProfilePictureUrl);
 }
 tailor.ProfilePictureUrl = profilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
>>>>>>> Stashed changes
 await _tailorRepository.UpdateAsync(tailor);
 }
 break;
 case "corporate":
 var corporate = await _corporateRepository.GetByUserIdAsync(userId);
 if (corporate != null)
 {
<<<<<<< Updated upstream
 corporate.ProfilePictureUrl = profilePictureUrl;
=======
 // Delete old profile picture if exists
#pragma warning disable CS0618 // Type or member is obsolete
 if (!string.IsNullOrEmpty(corporate.ProfilePictureUrl))
 {
 DeleteOldProfilePicture(corporate.ProfilePictureUrl);
 }
 corporate.ProfilePictureUrl = profilePictureUrl;
#pragma warning restore CS0618 // Type or member is obsolete
>>>>>>> Stashed changes
 await _corporateRepository.UpdateAsync(corporate);
 }
 break;
 }
<<<<<<< Updated upstream
 return true;
 }
 catch
 {
 return false;
 }
 }
=======

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
 string? oldPictureUrl = null;

 switch (role)
 {
 case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
 {
#pragma warning disable CS0618 // Type or member is obsolete
 oldPictureUrl = customer.ProfilePictureUrl;
 customer.ProfilePictureUrl = null;
#pragma warning restore CS0618 // Type or member is obsolete
 await _customerRepository.UpdateAsync(customer);
 }
 break;
 case "tailor":
 var tailor = await _tailorRepository.GetByUserIdAsync(userId);
 if (tailor != null)
 {
#pragma warning disable CS0618 // Type or member is obsolete
 oldPictureUrl = tailor.ProfilePictureUrl;
 tailor.ProfilePictureUrl = null;
#pragma warning restore CS0618 // Type or member is obsolete
 await _tailorRepository.UpdateAsync(tailor);
 }
 break;
 case "corporate":
 var corporate = await _corporateRepository.GetByUserIdAsync(userId);
 if (corporate != null)
 {
#pragma warning disable CS0618 // Type or member is obsolete
 oldPictureUrl = corporate.ProfilePictureUrl;
 corporate.ProfilePictureUrl = null;
#pragma warning restore CS0618 // Type or member is obsolete
 await _corporateRepository.UpdateAsync(corporate);
 }
 break;
 }

 // Delete physical file
 if (!string.IsNullOrEmpty(oldPictureUrl))
 {
 DeleteOldProfilePicture(oldPictureUrl);
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

 private void DeleteOldProfilePicture(string pictureUrl)
 {
 try
 {
 var physicalPath = Path.Combine(_environment.WebRootPath, pictureUrl.TrimStart('/'));
 if (File.Exists(physicalPath))
 {
 File.Delete(physicalPath);
 _logger.LogInformation("Deleted old profile picture: {Path}", physicalPath);
 }
 }
 catch (Exception ex)
 {
 _logger.LogWarning(ex, "Failed to delete old profile picture: {Url}", pictureUrl);
 // Don't throw - deletion failure shouldn't break the update
 }
 }
>>>>>>> Stashed changes
 }
}
