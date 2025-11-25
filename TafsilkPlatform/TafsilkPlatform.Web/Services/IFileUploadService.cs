namespace TafsilkPlatform.Web.Services;

public interface IFileUploadService
{
    /// <summary>
    /// Uploads a profile picture and returns the file path
    /// </summary>
    Task<string> UploadProfilePictureAsync(IFormFile file, string userId);

    /// <summary>
    /// Deletes a profile picture
    /// </summary>
    Task<bool> DeleteProfilePictureAsync(string filePath);

    /// <summary>
    /// Validates if the file is an image
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file);

    /// <summary>
    /// Checks if the image is valid
    /// </summary>
    Task<bool> IsValidImageAsync(IFormFile file);

    /// <summary>
    /// Gets the allowed file extensions
    /// </summary>
    string[] GetAllowedExtensions();

    /// <summary>
    /// Gets the maximum file size in bytes
    /// </summary>
    long GetMaxFileSizeInBytes();
}
