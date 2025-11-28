namespace TafsilkPlatform.Web.Services;

public class FileUploadService : IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public FileUploadService(ILogger<FileUploadService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        // Dummy implementation for interface compliance
        await Task.CompletedTask;
        return "";
    }

    public async Task<bool> DeleteProfilePictureAsync(string filePath)
    {
        // Dummy implementation for interface compliance
        await Task.CompletedTask;
        return true;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file)
    {
        // Basic validation for interface compliance
        await Task.CompletedTask;
        if (file == null || file.Length == 0)
            return (false, "الملف فارغ أو غير موجود.");
        if (file.Length > MaxFileSizeInBytes)
            return (false, $"حجم الملف كبير جداً. الحد الأقصى {MaxFileSizeInBytes / (1024 * 1024)} ميجابايت.");
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            return (false, $"امتداد الملف غير مدعوم: {extension}");
        return (true, null);
    }

    public async Task<bool> IsValidImageAsync(IFormFile file)
    {
        var validationResult = await ValidateImageAsync(file);
        return validationResult.IsValid;
    }

    public string[] GetAllowedExtensions() => _allowedExtensions;
    public long GetMaxFileSizeInBytes() => MaxFileSizeInBytes;
}
