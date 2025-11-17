using System.Security.Cryptography;
using System.Text;
using TafsilkPlatform.Shared.Constants;

namespace TafsilkPlatform.Shared.Utilities
{
    /// <summary>
    /// Shared password hashing utility
    /// </summary>
    public static class PasswordHasher
    {
     /// <summary>
     /// Hash password using SHA256
/// </summary>
      public static string HashPassword(string password)
        {
       if (string.IsNullOrWhiteSpace(password))
 throw new ArgumentException("Password cannot be empty", nameof(password));

   using var sha256 = SHA256.Create();
       var bytes = Encoding.UTF8.GetBytes(password);
         var hash = sha256.ComputeHash(bytes);
return Convert.ToBase64String(hash);
   }

   /// <summary>
        /// Verify password against hash
 /// </summary>
        public static bool VerifyPassword(string password, string hash)
   {
      if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
   return false;

       var hashedPassword = HashPassword(password);
       return hashedPassword == hash;
   }

  /// <summary>
  /// Generate a random password
 /// </summary>
     public static string GenerateRandomPassword(int length = 12)
        {
      const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
      var random = new Random();
var password = new char[length];
     
for (int i = 0; i < length; i++)
   {
     password[i] = validChars[random.Next(validChars.Length)];
       }

      return new string(password);
 }
    }

    /// <summary>
 /// Shared validation utilities
    /// </summary>
  public static class ValidationHelper
    {
   /// <summary>
 /// Validate Egyptian phone number
        /// </summary>
  public static bool IsValidEgyptianPhone(string phoneNumber)
    {
      if (string.IsNullOrWhiteSpace(phoneNumber))
 return false;

      var regex = new System.Text.RegularExpressions.Regex(AppConstants.Validation.EgyptianPhoneRegex);
 return regex.IsMatch(phoneNumber);
  }

  /// <summary>
/// Validate email address
   /// </summary>
        public static bool IsValidEmail(string email)
        {
    if (string.IsNullOrWhiteSpace(email))
  return false;

  try
{
 var addr = new System.Net.Mail.MailAddress(email);
     return addr.Address == email;
  }
 catch
  {
   return false;
      }
  }

     /// <summary>
/// Sanitize user input
     /// </summary>
    public static string SanitizeInput(string input)
    {
       if (string.IsNullOrWhiteSpace(input))
      return string.Empty;

   return input.Trim();
  }
    }

    /// <summary>
    /// Shared date/time utilities
    /// </summary>
    public static class DateTimeHelper
    {
   /// <summary>
   /// Get current UTC time
      /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
     /// Get current local time (Egypt - Cairo)
   /// </summary>
        public static DateTime EgyptNow
        {
   get
   {
  var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
   return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
     }
  }

  /// <summary>
    /// Format date for display in Arabic
    /// </summary>
  public static string FormatDateArabic(DateTime date)
   {
       return date.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("ar-EG"));
        }

     /// <summary>
        /// Calculate days between dates
 /// </summary>
     public static int DaysBetween(DateTime start, DateTime end)
   {
       return (end.Date - start.Date).Days;
}
    }

    /// <summary>
    /// Shared ID generation utility
/// </summary>
    public static class IdGenerator
    {
     /// <summary>
 /// Generate new GUID
   /// </summary>
        public static Guid NewGuid() => Guid.NewGuid();

   /// <summary>
     /// Generate short order ID
   /// </summary>
 public static string GenerateOrderId()
{
   return $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

     /// <summary>
/// Generate short service ID
 /// </summary>
     public static string GenerateServiceId()
   {
  return $"SRV-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
   }
  }
}
