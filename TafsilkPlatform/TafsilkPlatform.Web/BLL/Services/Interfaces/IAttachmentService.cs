namespace BLL.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<string> Upload(IFormFile file, string folderName);
        Task<bool> Delete(string filePath);
    }
}
