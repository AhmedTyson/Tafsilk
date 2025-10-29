using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.Services;

public class FileUploadService : IFileUploadService
{
  private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
_logger = logger;
  }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        try
      {
        if (file == null || file.Length == 0)
            {
       throw new ArgumentException("File is empty");
            }

  if (!IsValidImage(file))
     {
        throw new ArgumentException("Invalid file type. Only images are allowed.");
    }

       if (file.Length > MaxFileSizeInBytes)
     {
      throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSizeInBytes / 1024 / 1024}MB");
  }

     // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
        if (!Directory.Exists(uploadsPath))
    {
         Directory.CreateDirectory(uploadsPath);
   }

       // Generate unique filename
      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
      var filePath = Path.Combine(uploadsPath, fileName);

          // Save file
   using (var stream = new FileStream(filePath, FileMode.Create))
        {
     await file.CopyToAsync(stream);
    }

        // Return relative path for database storage
    return $"/uploads/profiles/{fileName}";
    }
catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteProfilePictureAsync(string filePath)
    {
        try
        {
     if (string.IsNullOrEmpty(filePath))
        {
        return false;
}

            // Convert relative path to absolute
            var absolutePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

    if (File.Exists(absolutePath))
            {
                await Task.Run(() => File.Delete(absolutePath));
             _logger.LogInformation("Deleted profile picture: {FilePath}", filePath);
  return true;
         }

          return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture: {FilePath}", filePath);
            return false;
  }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null)
        {
      return false;
      }

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
       return false;
        }

        // Check MIME type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
      {
          return false;
    }

        return true;
    }

    public string[] GetAllowedExtensions()
    {
        return _allowedExtensions;
    }

    public long GetMaxFileSizeInBytes()
    {
  return MaxFileSizeInBytes;
    }
}
