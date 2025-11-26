namespace BLL.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<string> Upload(IFormFile file, string folderName);
        Task<bool> Delete(string filePath);
        Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(IFormFile? file);
        Task<byte[]> ProcessToBytesAsync(IFormFile file);
    }
}
