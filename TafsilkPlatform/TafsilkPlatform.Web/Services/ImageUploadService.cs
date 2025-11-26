using BLL.Services.Interfaces;

namespace TafsilkPlatform.Web.Services;

public class ImageUploadService : IFileUploadService
{
    private readonly ILogger<ImageUploadService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10MB

    public ImageUploadService(ILogger<ImageUploadService> logger, IAttachmentService attachmentService)
    {
        _logger = logger;
        _attachmentService = attachmentService;
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        // Delegate to AttachmentService
        var relative = await _attachmentService.Upload(file, "profile");
        if (string.IsNullOrEmpty(relative))
        {
            throw new InvalidOperationException("فشل رفع الصورة إلى التخزين");
        }

        _logger.LogInformation("Profile picture uploaded for user {UserId}. File: {File}", userId, relative);
        return relative;
    }

    public async Task<bool> DeleteProfilePictureAsync(string filePath)
    {
        return await _attachmentService.Delete(filePath);
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file)
    {
        // Delegate to AttachmentService
        return await _attachmentService.ValidateFileAsync(file);
    }

    public async Task<byte[]> ProcessImageAsync(IFormFile file)
    {
        // Delegate to AttachmentService
        return await _attachmentService.ProcessToBytesAsync(file);
    }

    public async Task<byte[]> ProcessImageWithSizeCheckAsync(IFormFile file, long maxSize = MaxFileSizeInBytes)
    {
        // Delegate to AttachmentService (it handles size checks internally)
        // Note: AttachmentService uses its own constant for max size, which matches ours (10MB)
        return await _attachmentService.ProcessToBytesAsync(file);
    }

    public string SanitizeFileName(string fileName)
    {
        // Simple sanitization if needed, but AttachmentService handles this during upload
        if (string.IsNullOrWhiteSpace(fileName)) return Guid.NewGuid().ToString("N") + ".jpg";
        return Path.GetFileName(fileName);
    }

    public string GenerateUniqueFileName(string? originalFileName = null)
    {
        // AttachmentService handles this internally during upload
        var extension = Path.GetExtension(originalFileName ?? ".jpg");
        return $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}{extension}";
    }

    public long GetMaxFileSizeInBytes() => MaxFileSizeInBytes;

    public string[] GetAllowedExtensions() => _allowedExtensions;

    public async Task<bool> IsValidImageAsync(IFormFile file)
    {
        var result = await _attachmentService.ValidateFileAsync(file);
        return result.IsValid;
    }
}
