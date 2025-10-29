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
 }

 public class UserService : IUserService
 {
 private readonly AppDbContext _db;
 private readonly IUserRepository _userRepository;
 private readonly ICustomerRepository _customerRepository;
 private readonly ITailorRepository _tailorRepository;
 private readonly ICorporateRepository _corporateRepository;
 private readonly IWebHostEnvironment _environment;

 public UserService(
 AppDbContext db,
 IUserRepository userRepository,
 ICustomerRepository customerRepository,
 ITailorRepository tailorRepository,
 ICorporateRepository corporateRepository,
 IWebHostEnvironment environment)
 {
 _db = db;
 _userRepository = userRepository;
 _customerRepository = customerRepository;
 _tailorRepository = tailorRepository;
 _corporateRepository = corporateRepository;
 _environment = environment;
 }

 public async Task<UserSettingsViewModel?> GetUserSettingsAsync(Guid userId)
 {
 var user = await _db.Users
 .Include(u => u.Role)
 .FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null) return null;

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
 settings.FullName = customer.FullName;
 settings.City = customer.City;
 settings.DateOfBirth = customer.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
 settings.Bio = customer.Bio;
 settings.ProfilePictureUrl = customer.ProfilePictureUrl;
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
 settings.ProfilePictureUrl = tailor.ProfilePictureUrl;
 }
 break;
 case "corporate":
 var corporate = await _corporateRepository.GetByUserIdAsync(userId);
 if (corporate != null)
 {
 settings.CompanyName = corporate.CompanyName;
 settings.ContactPerson = corporate.ContactPerson;
 settings.Bio = corporate.Bio;
 settings.ProfilePictureUrl = corporate.ProfilePictureUrl;
 }
 break;
 }

 return settings;
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

 if (user.Email != request.Email && await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
 return (false, "البريد الإلكتروني مستخدم من قبل");

 if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber && await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
 return (false, "رقم الهاتف مستخدم من قبل");

 if (!string.IsNullOrEmpty(request.NewPassword))
 {
 if (string.IsNullOrEmpty(request.CurrentPassword))
 return (false, "يجب إدخال كلمة المرور الحالية");
 if (!PasswordHasher.Verify(user.PasswordHash, request.CurrentPassword))
 return (false, "كلمة المرور الحالية غير صحيحة");
 user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
 }

 user.Email = request.Email;
 user.PhoneNumber = request.PhoneNumber;
 user.EmailNotifications = request.EmailNotifications;
 user.SmsNotifications = request.SmsNotifications;
 user.PromotionalNotifications = request.PromotionalNotifications;
 user.UpdatedAt = DateTime.UtcNow;
 await _userRepository.UpdateAsync(user);

 var role = user.Role?.Name?.ToLower();
 switch (role)
 {
 case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
 {
 customer.FullName = request.FullName;
 customer.City = request.City;
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
 return (true, null);
 }
 catch
 {
 await transaction.RollbackAsync();
 return (false, "حدث خطأ أثناء حفظ التعديلات");
 }
 }

 public async Task<bool> UpdateProfilePictureAsync(Guid userId, IFormFile profilePicture)
 {
 try
 {
 if (profilePicture == null || profilePicture.Length ==0) return false;
 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
 var extension = Path.GetExtension(profilePicture.FileName).ToLower();
 if (!allowedExtensions.Contains(extension)) return false;
 if (profilePicture.Length >5 *1024 *1024) return false;

 var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
 if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
 var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
 var filePath = Path.Combine(uploadsPath, fileName);
 using (var stream = new FileStream(filePath, FileMode.Create))
 {
 await profilePicture.CopyToAsync(stream);
 }
 var profilePictureUrl = $"/uploads/profiles/{fileName}";

 var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
 if (user == null) return false;
 var role = user.Role?.Name?.ToLower();
 switch (role)
 {
 case "customer":
 var customer = await _customerRepository.GetByUserIdAsync(userId);
 if (customer != null)
 {
 customer.ProfilePictureUrl = profilePictureUrl;
 await _customerRepository.UpdateAsync(customer);
 }
 break;
 case "tailor":
 var tailor = await _tailorRepository.GetByUserIdAsync(userId);
 if (tailor != null)
 {
 tailor.ProfilePictureUrl = profilePictureUrl;
 await _tailorRepository.UpdateAsync(tailor);
 }
 break;
 case "corporate":
 var corporate = await _corporateRepository.GetByUserIdAsync(userId);
 if (corporate != null)
 {
 corporate.ProfilePictureUrl = profilePictureUrl;
 await _corporateRepository.UpdateAsync(corporate);
 }
 break;
 }
 return true;
 }
 catch
 {
 return false;
 }
 }
 }
}
