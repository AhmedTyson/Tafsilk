using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<string> Upload(IFormFile file, string folderName);
        Task<bool> Delete(string filePath);
    }
}
