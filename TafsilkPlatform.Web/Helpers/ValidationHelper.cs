using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Helpers;

/// <summary>
/// Simple validation helpers - makes validation easy
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return new EmailAddressAttribute().IsValid(email);
    }

    /// <summary>
    /// Validates phone number (Saudi format)
    /// </summary>
    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        // Saudi phone: starts with 05, 10 digits total
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^05\d{8}$");
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "كلمة المرور مطلوبة");

        if (password.Length < 8)
            return (false, "كلمة المرور يجب أن تكون 8 أحرف على الأقل");

        if (!password.Any(char.IsUpper))
            return (false, "كلمة المرور يجب أن تحتوي على حرف كبير على الأقل");

        if (!password.Any(char.IsLower))
            return (false, "كلمة المرور يجب أن تحتوي على حرف صغير على الأقل");

        if (!password.Any(char.IsDigit))
            return (false, "كلمة المرور يجب أن تحتوي على رقم على الأقل");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates GUID format
    /// </summary>
    public static bool IsValidGuid(string? guidString)
    {
        if (string.IsNullOrWhiteSpace(guidString)) return false;
        return Guid.TryParse(guidString, out _);
    }
}

