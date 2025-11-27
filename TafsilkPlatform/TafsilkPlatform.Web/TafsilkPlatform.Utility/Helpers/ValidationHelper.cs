using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Utility.Helpers;

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
    /// Validates phone number (Egyptian format)
    /// </summary>
    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        // Egyptian phone: starts with 010, 011, 012, 015, 11 digits total
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^01[0125]\d{8}$");
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters long");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one digit");

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

