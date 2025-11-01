// SECURE FILE UPLOAD VALIDATION SERVICE
using System.Security.Cryptography;

namespace TafsilkPlatform.Web.Services;

public interface IFileValidationService
{
    Task<(bool IsValid, string? Error)> ValidateImageAsync(IFormFile file);
    bool ValidateFileExtension(string fileName, string[] allowedExtensions);
    bool ValidateFileSize(long fileSize, long maxSizeInBytes);
}

public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    
    // Magic numbers for common image formats
    private static readonly Dictionary<string, byte[][]> _fileSignatures = new()
    {
        // JPEG
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
 { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        
        // PNG
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
 
        // GIF
    { ".gif", new[] { 
        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
        } },
        
        // PDF (for ID documents)
   { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } } // %PDF
 };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private static readonly string[] AllowedDocumentExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<(bool IsValid, string? Error)> ValidateImageAsync(IFormFile file)
    {
        // 1. Check file is not null
        if (file == null || file.Length == 0)
      {
 return (false, "File is empty");
      }

        // 2. Validate file size (5MB limit)
     if (!ValidateFileSize(file.Length, MaxFileSizeBytes))
 {
      return (false, $"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB");
        }

        // 3. Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ValidateFileExtension(file.FileName, AllowedImageExtensions))
        {
            return (false, $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedImageExtensions)}");
        }

        // 4. Validate magic number (file signature)
        if (!_fileSignatures.ContainsKey(extension))
        {
            return (false, $"Unknown file type: {extension}");
        }

        using var reader = new BinaryReader(file.OpenReadStream());
        var signatures = _fileSignatures[extension];
        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

        bool isValidSignature = signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));

     if (!isValidSignature)
        {
   _logger.LogWarning("File upload rejected: Invalid magic number for extension {Extension}", extension);
return (false, "File content does not match its extension. Possible file spoofing attempt.");
        }

        // 5. Additional security: Check for embedded executables (basic check)
file.OpenReadStream().Seek(0, SeekOrigin.Begin);
    using var memoryStream = new MemoryStream();
   await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

    // Check for MZ header (Windows executable)
        if (fileBytes.Length > 2 && fileBytes[0] == 0x4D && fileBytes[1] == 0x5A)
        {
            _logger.LogWarning("File upload rejected: Executable detected in image file");
        return (false, "File contains executable code and is not allowed.");
      }

    // 6. Generate secure filename
        var safeFileName = GenerateSafeFileName(file.FileName);
        _logger.LogInformation("File validation passed: {FileName} -> {SafeFileName}", file.FileName, safeFileName);

        return (true, null);
    }

    public bool ValidateFileExtension(string fileName, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    public bool ValidateFileSize(long fileSize, long maxSizeInBytes)
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }

    public string GenerateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
     
        // Generate cryptographically secure random filename
        var randomBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
  {
     rng.GetBytes(randomBytes);
        }
     
        var safeFileName = $"{BitConverter.ToString(randomBytes).Replace("-", "").ToLower()}{extension}";
        return safeFileName;
    }

    public async Task<(bool IsValid, string? Error)> ValidateDocumentAsync(IFormFile file)
    {
        // Similar to ValidateImageAsync but allows PDF
        if (file == null || file.Length == 0)
        {
            return (false, "File is empty");
      }

        if (!ValidateFileSize(file.Length, MaxFileSizeBytes))
        {
            return (false, $"File size exceeds {MaxFileSizeBytes / (1024 * 1024)}MB");
     }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ValidateFileExtension(file.FileName, AllowedDocumentExtensions))
        {
     return (false, $"File type not allowed: {extension}");
        }

        // Validate magic number
        if (!_fileSignatures.ContainsKey(extension))
    {
     return (false, $"Unknown file type: {extension}");
        }

        using var reader = new BinaryReader(file.OpenReadStream());
        var signatures = _fileSignatures[extension];
        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

        bool isValidSignature = signatures.Any(signature =>
    headerBytes.Take(signature.Length).SequenceEqual(signature));

        if (!isValidSignature)
        {
        return (false, "Invalid file signature");
        }

        return (true, null);
    }
}

// USAGE IN ACCOUNTCONTROLLER:
/*
// Inject service
private readonly IFileValidationService _fileValidation;

// In ProvideTailorEvidence POST:
// Validate ID document
var (isValidId, idError) = await _fileValidation.ValidateDocumentAsync(model.IdDocument);
if (!isValidId)
{
    ModelState.AddModelError(nameof(model.IdDocument), idError ?? "Invalid document file");
    return View(model);
}

// Validate portfolio images
if (model.PortfolioImages != null)
{
    foreach (var image in model.PortfolioImages)
    {
        var (isValidImg, imgError) = await _fileValidation.ValidateImageAsync(image);
        if (!isValidImg)
        {
      ModelState.AddModelError(nameof(model.PortfolioImages), imgError ?? "Invalid image file");
      return View(model);
      }
    }
}

// Generate secure filename
var safeFileName = _fileValidation.GenerateSafeFileName(model.IdDocument.FileName);
*/
