using BLL.Services.Interfaces;
using System.Text;

namespace BLL.Services
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".pdf", ".webp", ".gif"
        };

        private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "profile", "order", "gallery", "product"
        };

        // Magic bytes (file signatures) for validation
        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } } },
            { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }, // RIFF header
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };

        public const int MaxAllowedBytes = 10_000_000; // 10 MB
        private const int BufferSize = 8192;

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttachmentService> _logger;

        public AttachmentService(IWebHostEnvironment env, ILogger<AttachmentService> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads the file into wwwroot/Attachments/{folderName}.
        /// Validates the file first. Returns relative path on success.
        /// </summary>
        public async Task<string> Upload(IFormFile file, string folderName)
        {
            try
            {
                if (file == null) return null;
                if (string.IsNullOrWhiteSpace(folderName)) return null;

                var folderKey = folderName.Trim().ToLowerInvariant();
                if (!AllowedFolders.Contains(folderKey))
                {
                    _logger.LogWarning("Attempt to upload to non-allowed folder: {Folder}", folderName);
                    return null;
                }

                // 1. Validate File (Size, Extension, Magic Bytes)
                var (isValid, errorMessage) = await ValidateFileAsync(file);
                if (!isValid)
                {
                    _logger.LogWarning("Upload rejected: {Error}", errorMessage);
                    return null;
                }

                // 2. Prepare Paths
                var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                    ? Path.Combine(_env.ContentRootPath, "wwwroot")
                    : _env.WebRootPath;

                var attachmentsRoot = Path.Combine(webRoot, "Attachments");
                var folderPath = Path.Combine(attachmentsRoot, folderKey);
                Directory.CreateDirectory(folderPath);

                // 3. Generate Unique Filename
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var fileName = $"{timestamp}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                // 4. Security Check
                var fullAttachmentsRoot = Path.GetFullPath(attachmentsRoot);
                var fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullAttachmentsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Resolved file path is outside attachments root: {Path}", fullFilePath);
                    return null;
                }

                // 5. Save File (Safe Stream Handling)
                await using var fileStream = new FileStream(
                    fullFilePath,
                    FileMode.Create, // Overwrite if exists (safe due to unique name)
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: BufferSize,
                    useAsync: true);

                await file.CopyToAsync(fileStream);
                await fileStream.FlushAsync();

                // 6. Return Relative Path
                var relativePath = Path.Combine("Attachments", folderKey, fileName).Replace('\\', '/');
                _logger.LogInformation("Successfully uploaded file to: {Path}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to folder {Folder}", file?.FileName, folderName);
                return null;
            }
        }

        /// <summary>
        /// Deletes the file if it's under wwwroot/Attachments and inside an allowed folder.
        /// </summary>
        public Task<bool> Delete(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return Task.FromResult(false);

                var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                    ? Path.Combine(_env.ContentRootPath, "wwwroot")
                    : _env.WebRootPath;

                var attachmentsRoot = Path.Combine(webRoot, "Attachments");

                // Normalize path
                string candidateRelative;
                if (Path.IsPathRooted(filePath))
                {
                    var full = Path.GetFullPath(filePath);
                    if (!full.StartsWith(Path.GetFullPath(attachmentsRoot), StringComparison.OrdinalIgnoreCase))
                        return Task.FromResult(false);
                    candidateRelative = full.Substring(Path.GetFullPath(attachmentsRoot).Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }
                else
                {
                    candidateRelative = filePath
                        .Replace("Attachments/", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("Attachments\\", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }

                var firstSegment = candidateRelative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (string.IsNullOrEmpty(firstSegment) || !AllowedFolders.Contains(firstSegment))
                    return Task.FromResult(false);

                var fullPath = Path.Combine(attachmentsRoot, candidateRelative);
                if (!File.Exists(fullPath)) return Task.FromResult(false);

                File.Delete(fullPath);
                _logger.LogInformation("Successfully deleted file: {Path}", filePath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Validates file size, extension, and magic bytes.
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(IFormFile? file)
        {
            _logger.LogCritical("ValidateFileAsync started for {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
                return (false, "File is empty.");

            if (file.Length > MaxAllowedBytes)
                return (false, $"File size exceeds limit of {MaxAllowedBytes / (1024 * 1024)}MB.");

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return (false, "Invalid file extension.");

            // Magic Byte Validation
            if (FileSignatures.TryGetValue(extension, out var signatures))
            {
                Stream? stream = null;
                try
                {
                    stream = file.OpenReadStream();
                    if (stream.CanSeek) stream.Position = 0;

                    var headerBytes = new byte[12];
                    var bytesRead = await stream.ReadAsync(headerBytes, 0, headerBytes.Length);

                    bool signatureMatch = false;
                    foreach (var signature in signatures)
                    {
                        if (bytesRead >= signature.Length)
                        {
                            bool match = true;
                            for (int i = 0; i < signature.Length; i++)
                            {
                                if (headerBytes[i] != signature[i])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                // Special check for WEBP
                                if (extension == ".webp")
                                {
                                    if (bytesRead >= 12)
                                    {
                                        var webpCheck = Encoding.ASCII.GetString(headerBytes, 8, 4);
                                        if (webpCheck == "WEBP") signatureMatch = true;
                                    }
                                }
                                else
                                {
                                    signatureMatch = true;
                                }
                                if (signatureMatch) break;
                            }
                        }
                    }

                    if (!signatureMatch)
                        return (false, "File signature verification failed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating file signature for {FileName}", file.FileName);
                    return (false, "Error validating file signature.");
                }
                finally
                {
                    if (stream != null) await stream.DisposeAsync();
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Reads file into byte array safely.
        /// </summary>
        public async Task<byte[]> ProcessToBytesAsync(IFormFile file)
        {
            _logger.LogCritical("ProcessToBytesAsync started for {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            if (file.Length > MaxAllowedBytes)
                throw new ArgumentException($"File exceeds {MaxAllowedBytes / (1024 * 1024)}MB limit");

            MemoryStream? memoryStream = null;
            Stream? fileStream = null;

            try
            {
                memoryStream = new MemoryStream();
                fileStream = file.OpenReadStream();
                if (fileStream.CanSeek) fileStream.Position = 0;

                await fileStream.CopyToAsync(memoryStream, BufferSize);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file to bytes: {FileName}", file.FileName);
                throw;
            }
            finally
            {
                if (fileStream != null) await fileStream.DisposeAsync();
                if (memoryStream != null) await memoryStream.DisposeAsync();
            }
        }
    }
}
