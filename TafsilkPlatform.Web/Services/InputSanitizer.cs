using System.Text.RegularExpressions;

namespace TafsilkPlatform.Web.Services;

public interface IInputSanitizer
{
    string SanitizeHtml(string input);
    string SanitizeFileName(string fileName);
    string SanitizeEmail(string email);
    bool IsValidPhoneNumber(string phoneNumber);
    bool ContainsSuspiciousContent(string input);
}

public class InputSanitizer : IInputSanitizer
{
    private static readonly Regex HtmlTagRegex = new(@"<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex ScriptRegex = new(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SqlInjectionRegex = new(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EgyptianPhoneRegex = new(@"^01[0-2,5]\d{8}$", RegexOptions.Compiled);
    private static readonly Regex FileNameRegex = new(@"[^a-zA-Z0-9\u0600-\u06FF._-]", RegexOptions.Compiled);

    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
      return string.Empty;

 // Remove script tags first
        var sanitized = ScriptRegex.Replace(input, string.Empty);
        
 // Remove all HTML tags
        sanitized = HtmlTagRegex.Replace(sanitized, string.Empty);
        
        // Decode HTML entities
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);
     
// Trim and normalize whitespace
        sanitized = Regex.Replace(sanitized.Trim(), @"\s+", " ");
      
      return sanitized;
    }

    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
    return "unnamed";

        // Get extension
        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        
        // Remove invalid characters (keep Arabic, English, numbers, and basic symbols)
        nameWithoutExt = FileNameRegex.Replace(nameWithoutExt, "_");
        
        // Limit length
        if (nameWithoutExt.Length > 50)
  nameWithoutExt = nameWithoutExt.Substring(0, 50);
        
        return nameWithoutExt + extension;
    }

    public string SanitizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        return email.Trim().ToLowerInvariant();
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
  return false;

        // Remove spaces and dashes
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "");
        
     return EgyptianPhoneRegex.IsMatch(cleaned);
    }

 public bool ContainsSuspiciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
       return false;

        // Check for SQL injection patterns
        if (SqlInjectionRegex.IsMatch(input))
   return true;

 // Check for XSS patterns
    if (ScriptRegex.IsMatch(input))
       return true;

        // Check for excessive special characters (potential encoding attack)
        var specialCharCount = input.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
      if (specialCharCount > input.Length * 0.3) // More than 30% special chars
            return true;

        return false;
    }
}
