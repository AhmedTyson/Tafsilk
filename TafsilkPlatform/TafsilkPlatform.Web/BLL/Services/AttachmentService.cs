using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf"
        };

        private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "profile",
            "order",
            "gallery",
            "product"
        };

        public const int maxAllowed = 2_000_000; // 2 MB

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttachmentService> _logger;

        public AttachmentService(IWebHostEnvironment env, ILogger<AttachmentService> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads the file into wwwroot/Attachments/{folderName}.
        /// Only folders in AllowedFolders are permitted. Returns relative path under Attachments (e.g. "Attachments/profile/filename.jpg") on success.
        /// On error, returns null and logs the issue.
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

                // 1-Get Extension & validation
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Rejected upload due to invalid extension: {FileName}", file.FileName);
                    return null;
                }

                //2-size
                if (file.Length > maxAllowed)
                {
                    _logger.LogWarning("Rejected upload due to size limit. Size: {Size} bytes, Max: {Max}", file.Length, maxAllowed);
                    return null;
                }

                //3. get located folder path (wwwroot/Attachments/<folderKey>)
                var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                    ? Path.Combine(_env.ContentRootPath, "wwwroot")
                    : _env.WebRootPath;

                var attachmentsRoot = Path.Combine(webRoot, "Attachments");
                var folderPath = Path.Combine(attachmentsRoot, folderKey);
                Directory.CreateDirectory(folderPath);

                //4. SET UNIQUE FILE NAME
                var fileName = $"{Guid.NewGuid()}{extension}";

                //5. Get file path
                var filePath = Path.Combine(folderPath, fileName);

                // Security: ensure resulting path is inside attachmentsRoot
                var fullAttachmentsRoot = Path.GetFullPath(attachmentsRoot);
                var fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullAttachmentsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Resolved file path is outside attachments root: {Path}", fullFilePath);
                    return null;
                }

                //6. save file as stream - use asynchronous file options
                await using var fileStream = new FileStream(fullFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                //7. copy file to stream
                await file.CopyToAsync(fileStream);

                //8.return relative path under Attachments
                var relativePath = Path.Combine("Attachments", folderKey, fileName).Replace('\\', '/');
                return relativePath;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "IO error while uploading file {FileName} to folder {Folder}", file?.FileName, folderName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while uploading file {FileName} to folder {Folder}", file?.FileName, folderName);
                return null;
            }
        }

        /// <summary>
        /// Deletes the file if it's under wwwroot/Attachments and inside an allowed folder.
        /// Accepts either a relative path (Attachments/profile/xxx) or a path relative to the Attachments folder (profile/xxx) or an absolute path.
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

                string candidateRelative;
                // Normalize input to a path relative to Attachments
                if (Path.IsPathRooted(filePath))
                {
                    // If absolute, ensure it is under attachmentsRoot
                    var full = Path.GetFullPath(filePath);
                    if (!full.StartsWith(Path.GetFullPath(attachmentsRoot), StringComparison.OrdinalIgnoreCase))
                        return Task.FromResult(false);

                    candidateRelative = full.Substring(Path.GetFullPath(attachmentsRoot).Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }
                else
                {
                    candidateRelative = filePath.Replace("Attachments/", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("Attachments\\", string.Empty, StringComparison.OrdinalIgnoreCase).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }

                // first segment should be allowed folder
                var firstSegment = candidateRelative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (string.IsNullOrEmpty(firstSegment) || !AllowedFolders.Contains(firstSegment))
                    return Task.FromResult(false);

                var fullPath = Path.Combine(attachmentsRoot, candidateRelative);
                if (!File.Exists(fullPath)) return Task.FromResult(false);

                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }
    }
}
