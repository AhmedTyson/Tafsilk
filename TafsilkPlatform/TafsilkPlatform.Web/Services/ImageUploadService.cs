using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using BLL.Services.Interfaces;

namespace TafsilkPlatform.Web.Services;

public class ImageUploadService : IFileUploadService
{
    private readonly ILogger<ImageUploadService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IAttachmentService _attachmentService;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
    private const int BufferSize = 8192; // 8KB buffer for efficient streaming

    // Magic bytes (file signatures) for image validation
    private static readonly Dictionary<string, byte[][]> ImageSignatures = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // RIFF header, need to check for WEBP
    };

    public ImageUploadService(ILogger<ImageUploadService> logger, IHostEnvironment environment, IAttachmentService attachmentService)
    {
        _logger = logger;
        _environment = environment;
        _attachmentService = attachmentService;
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        var validationResult = await ValidateImageAsync(file);
        if (!validationResult.IsValid)
            throw new ArgumentException(validationResult.ErrorMessage);

        // Use attachment service to store under Attachments/profile
        var relative = await _attachmentService.Upload(file, "profile");
        if (string.IsNullOrEmpty(relative))
        {
            throw new InvalidOperationException("فشل رفع الصورة إلى التخزين");
        }

        _logger.LogInformation("Profile picture uploaded for user {UserId}. File: {File}", userId, relative);
        return relative; // e.g. "Attachments/profile/{filename}"
    }

    public async Task<bool> DeleteProfilePictureAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("DeleteProfilePictureAsync: File path is null or empty");
                return false;
            }

            var deleted = await _attachmentService.Delete(filePath);
            if (deleted)
                _logger.LogInformation("DeleteProfilePictureAsync: File deleted: {FilePath}", filePath);
            else
                _logger.LogWarning("DeleteProfilePictureAsync: File not found or not allowed: {FilePath}", filePath);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteProfilePictureAsync: Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// Validates image file using best practices:
    /// 1. File existence and size
    /// 2. Extension validation
    /// 3. MIME type validation
    /// 4. File signature (magic bytes) validation - prevents spoofing
    /// Returns a friendly, specific error message when validation fails.
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file)
    {
        // 🔴 BREAKPOINT #1: Set breakpoint here to inspect incoming file (Debug mode only)
        #if DEBUG
        // Debugger.Break(); // Uncomment to enable breakpoint during debugging
        #endif
        
        try
        {
            // Step 1: Basic null/empty check
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("ValidateImageAsync: File is null or empty. IsNull: {IsNull}, Length: {Length}", 
                    file == null, file?.Length ?? 0);
                return (false, "الملف فارغ أو غير موجود. الرجاء اختيار ملف صورة صالح.");
            }

            _logger.LogInformation("ValidateImageAsync: Starting validation for file {FileName}, Size: {Size} bytes", 
                file.FileName, file.Length);

            // Step 2: File size validation
            if (file.Length > MaxFileSizeInBytes)
            {
                var maxSizeMB = MaxFileSizeInBytes / (1024 * 1024);
                _logger.LogWarning("ValidateImageAsync: File too large. Size: {Size}, Max: {Max}", 
                    file.Length, MaxFileSizeInBytes);
                return (false, $"حجم الملف كبير جداً ({Math.Round(file.Length / (1024d * 1024d), 2)} MB). الحد الأقصى المسموح به هو {maxSizeMB} ميجابايت.");
            }

            // Step 3: Extension validation
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("ValidateImageAsync: Invalid extension: {Extension}", extension);
                return (false, $"امتداد الملف غير مدعوم (مُدخل: '{extension}'). الامتدادات المسموح بها: {string.Join(", ", _allowedExtensions)}.");
            }

            // Step 4: MIME type validation
            var contentType = file.ContentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !_allowedMimeTypes.Contains(contentType))
            {
                _logger.LogWarning("ValidateImageAsync: Invalid MIME type: {ContentType} for file: {FileName}", contentType, file.FileName);
                return (false, $"نوع المحتوى (MIME) غير صالح ('{contentType}'). يرجى التأكد من رفع صورة بصيغة صحيحة.");
            }

            // 🔴 BREAKPOINT #2: Set breakpoint here before file signature validation (Debug mode only)
            #if DEBUG
            // Debugger.Break(); // Uncomment to enable breakpoint during debugging
            #endif
            
            // Step 5: File signature (magic bytes) validation - CRITICAL for security
            var isValidSignature = await ValidateFileSignatureAsync(file, extension);
            if (!isValidSignature)
            {
                _logger.LogWarning("ValidateImageAsync: File signature validation failed for: {FileName}. Extension: {Extension}, ContentType: {ContentType}",
                    file.FileName, extension, contentType);
                return (false, "الملف يبدو تالفاً أو لا يتوافق مع نوع الصورة المُعلن. يرجى اختيار ملف صورة صالح.");
            }

            _logger.LogInformation("ValidateImageAsync: Validation successful for {FileName}", file.FileName);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateImageAsync: Unexpected error validating file: {FileName}", file?.FileName);
            // Return a clear, non-technical message to the caller but log the exception details
            return (false, "حدث خطأ أثناء التحقق من الملف. يرجى المحاولة مرة أخرى أو اختيار ملف آخر.");
        }
    }

    /// <summary>
    /// Validates file signature (magic bytes) to prevent file spoofing
    /// Returns false without throwing for known validation failures.
    /// </summary>
    private async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension)
    {
        // 🔴 BREAKPOINT #3: Set breakpoint here to inspect signature validation (Debug mode only)
        #if DEBUG
        // Debugger.Break(); // Uncomment to enable breakpoint during debugging
        #endif
        
        try
        {
            if (!ImageSignatures.TryGetValue(extension, out var signatures))
            {
                _logger.LogWarning("ValidateFileSignatureAsync: No signature configured for extension: {Extension}", extension);
                return false;
            }

            _logger.LogInformation("ValidateFileSignatureAsync: Validating signature for {Extension}", extension);

            await using var stream = file.OpenReadStream();

            var maxBytesToRead = (int)Math.Min(12, file.Length);
            if (maxBytesToRead == 0)
            {
                _logger.LogWarning("ValidateFileSignatureAsync: File length is zero for {FileName}", file.FileName);
                return false;
            }

            var buffer = new byte[maxBytesToRead];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, maxBytesToRead));

            if (bytesRead == 0)
            {
                _logger.LogWarning("ValidateFileSignatureAsync: Could not read any bytes from stream for {FileName}", file.FileName);
                return false;
            }

            _logger.LogInformation("ValidateFileSignatureAsync: Read {BytesRead} bytes from stream for {FileName}", bytesRead, file.FileName);

            foreach (var signature in signatures)
            {
                if (bytesRead >= signature.Length)
                {
                    var matches = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (buffer[i] != signature[i])
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        // Special check for WEBP (RIFF...WEBP)
                        if (extension == ".webp" && bytesRead >= 12)
                        {
                            var webpCheck = Encoding.ASCII.GetString(buffer, 8, 4);
                            if (webpCheck != "WEBP")
                            {
                                _logger.LogWarning("ValidateFileSignatureAsync: WEBP check failed for {FileName}", file.FileName);
                                return false;
                            }
                        }

                        _logger.LogInformation("ValidateFileSignatureAsync: Signature validation successful for {FileName}", file.FileName);
                        return true;
                    }
                }
            }

            _logger.LogWarning("ValidateFileSignatureAsync: No matching signature found for {FileName}", file.FileName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateFileSignatureAsync: Error validating file signature for: {FileName}", file.FileName);
            return false;
        }
    }

    /// <summary>
    /// Processes image file with memory-efficient streaming
    /// Throws ArgumentException with user-friendly message for known issues.
    /// </summary>
    public async Task<byte[]> ProcessImageAsync(IFormFile file)
    {
        // 🔴 BREAKPOINT #4: Set breakpoint here before processing image (Debug mode only)
        #if DEBUG
        // Debugger.Break(); // Uncomment to enable breakpoint during debugging
        #endif
        
        if (file == null || file.Length == 0)
        {
            _logger.LogError("ProcessImageAsync: File is null or empty");
            throw new ArgumentException("الملف فارغ أو غير موجود. يرجى اختيار صورة صحيحة.");
        }

        _logger.LogInformation("ProcessImageAsync: Starting to process image. Size: {Size} bytes", file.Length);

        try
        {
            using var memoryStream = new MemoryStream();
            await using var fileStream = file.OpenReadStream();

            _logger.LogInformation("ProcessImageAsync: Streams opened successfully for {FileName}", file.FileName);

            await fileStream.CopyToAsync(memoryStream, BufferSize);

            var result = memoryStream.ToArray();
            if (result.Length == 0)
            {
                _logger.LogWarning("ProcessImageAsync: Resulting image data is empty for {FileName}", file.FileName);
                throw new InvalidOperationException("فشل قراءة بيانات الصورة. قد يكون الملف تالفاً.");
            }

            _logger.LogInformation("ProcessImageAsync: Successfully processed image. Result size: {Size} bytes", result.Length);
            return result;
        }
        catch (OutOfMemoryException oomEx)
        {
            _logger.LogError(oomEx, "ProcessImageAsync: OUT OF MEMORY processing file {FileName}, Size: {Size} bytes", file.FileName, file.Length);
            throw new ArgumentException($"الملف كبير جداً ({Math.Round(file.Length / (1024d * 1024d), 2)} MB). يرجى تقليل حجم الصورة والمحاولة مرة أخرى.");
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "ProcessImageAsync: IO error processing file {FileName}", file.FileName);
            throw new ArgumentException("حدث خطأ أثناء قراءة الملف. يرجى المحاولة مرة أخرى.");
        }
        catch (ArgumentException)
        {
            throw; // rethrow user-facing argument exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessImageAsync: Unexpected error processing file {FileName}", file.FileName);
            throw new InvalidOperationException("حدث خطأ غير متوقع أثناء معالجة الصورة. يرجى المحاولة مرة أخرى.", ex);
        }
    }

    /// <summary>
    /// Processes image with size limit check during streaming
    /// Throws ArgumentException with clear message when size limit exceeded.
    /// </summary>
    public async Task<byte[]> ProcessImageWithSizeCheckAsync(IFormFile file, long maxSize = MaxFileSizeInBytes)
    {
        // 🔴 BREAKPOINT #5: Set breakpoint here before processing with size check (Debug mode only)
        #if DEBUG
        // Debugger.Break(); // Uncomment to enable breakpoint during debugging
        #endif
        
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("الملف فارغ أو غير موجود. يرجى اختيار صورة صحيحة.");
        }

        if (file.Length > maxSize)
        {
            _logger.LogWarning("ProcessImageWithSizeCheckAsync: File size {Size} exceeds max {Max}", file.Length, maxSize);
            throw new ArgumentException($"حجم الملف ({Math.Round(file.Length / (1024d * 1024d), 2)} MB) يتجاوز الحد المسموح به ({Math.Round(maxSize / (1024d * 1024d), 2)} MB). يرجى اختيار ملف أصغر.");
        }

        _logger.LogInformation("ProcessImageWithSizeCheckAsync: Processing with size check. Size: {Size}, Max: {Max}", file.Length, maxSize);

        try
        {
            using var memoryStream = new MemoryStream();
            await using var fileStream = file.OpenReadStream();

            var buffer = new byte[BufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead > maxSize)
                {
                    _logger.LogError("ProcessImageWithSizeCheckAsync: Size limit exceeded during streaming. Total: {Total}, Max: {Max}", totalBytesRead, maxSize);
                    throw new ArgumentException($"حجم الملف أثناء القراءة ({Math.Round(totalBytesRead / (1024d * 1024d), 2)} MB) تجاوز الحد المسموح به ({Math.Round(maxSize / (1024d * 1024d), 2)} MB). يرجى اختيار ملف أصغر.");
                }

                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            var result = memoryStream.ToArray();
            if (result.Length == 0)
            {
                _logger.LogWarning("ProcessImageWithSizeCheckAsync: Resulting image data is empty for {FileName}", file.FileName);
                throw new InvalidOperationException("فشل قراءة بيانات الصورة. قد يكون الملف تالفاً.");
            }

            _logger.LogInformation("ProcessImageWithSizeCheckAsync: Successfully processed. Total bytes: {Total}", totalBytesRead);
            return result;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "ProcessImageWithSizeCheckAsync: IO error processing file {FileName}", file.FileName);
            throw new ArgumentException("حدث خطأ أثناء قراءة الملف. يرجى المحاولة مرة أخرى.");
        }
        catch (ArgumentException)
        {
            throw; // rethrow to keep messages clear to caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessImageWithSizeCheckAsync: Unexpected error processing file {FileName}", file.FileName);
            throw new InvalidOperationException("حدث خطأ غير متوقع أثناء معالجة الصورة. يرجى المحاولة مرة أخرى.", ex);
        }
    }

    /// <summary>
    /// Sanitizes file name to prevent security issues
    /// </summary>
    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Guid.NewGuid().ToString("N") + ".jpg";
        }

        // Remove path components
        fileName = Path.GetFileName(fileName);

        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }

        // Limit length
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (nameWithoutExtension.Length > 100)
        {
            nameWithoutExtension = nameWithoutExtension.Substring(0, 100);
        }

        var extension = Path.GetExtension(fileName);
        return $"{nameWithoutExtension}{extension}";
    }

    /// <summary>
    /// Generates a secure, unique file name
    /// </summary>
    public string GenerateUniqueFileName(string? originalFileName = null)
    {
        var extension = !string.IsNullOrEmpty(originalFileName) 
            ? Path.GetExtension(originalFileName).ToLowerInvariant()
            : ".jpg";

        // Ensure extension is valid
        if (!_allowedExtensions.Contains(extension))
        {
            extension = ".jpg";
        }

        // Generate unique name with timestamp and GUID
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"{timestamp}_{guid}{extension}";
    }

    /// <summary>
    /// Gets maximum file size in bytes
    /// </summary>
    public long GetMaxFileSizeInBytes() => MaxFileSizeInBytes;

    /// <summary>
    /// Gets allowed file extensions
    /// </summary>
    public string[] GetAllowedExtensions() => _allowedExtensions;

    public bool IsValidImage(IFormFile file)
    {
        var validationTask = ValidateImageAsync(file);
        return validationTask.GetAwaiter().GetResult().IsValid;
    }
}

