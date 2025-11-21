using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Web.Helpers;

/// <summary>
/// Simple helper methods for error handling - makes error messages clear
/// </summary>
public static class ErrorHelper
{
    /// <summary>
    /// Creates a user-friendly error message
    /// </summary>
    public static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => "ليس لديك صلاحية للوصول إلى هذه الصفحة",
            ArgumentNullException => "بيانات غير مكتملة. يرجى التحقق من المدخلات",
            InvalidOperationException => ex.Message.Contains("configured") 
                ? ex.Message 
                : "عملية غير صالحة. يرجى المحاولة مرة أخرى",
            DbUpdateException => "حدث خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى",
            _ => "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى أو الاتصال بالدعم"
        };
    }

    /// <summary>
    /// Logs error with context in a simple format
    /// </summary>
    public static void LogError(ILogger logger, Exception ex, string context, params object[] args)
    {
        logger.LogError(ex, $"[{context}] Error: {{Message}}", ex.Message);
        if (args.Any())
        {
            logger.LogError($"[{context}] Context: {string.Join(", ", args)}");
        }
    }
}

