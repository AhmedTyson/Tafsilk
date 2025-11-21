using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.TailorManagement;
using Xunit;

namespace TafsilkPlatform.Web.Tests
{
    public class ProductManagementServiceTests
    {
        [Fact]
        public async Task ReadFormFileAsBytesAsync_ReturnsBytes_WhenFileProvided()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockFileService = new Mock<IFileUploadService>();
            var logger = new NullLogger<ProductManagementService>();

            var service = new ProductManagementService(mockUnitOfWork.Object, mockFileService.Object, logger);

            var content = new byte[] { 1, 2, 3, 4, 5 };
            var ms = new MemoryStream(content);
            var formFile = new FormFile(ms, 0, ms.Length, "Data", "test.jpg")
            {
                Headers = new Microsoft.AspNetCore.Http.HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await service.ReadFormFileAsBytesAsync(formFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(content.Length, result.Length);
        }

        [Fact]
        public async Task ReadFormFileAsBytesAsync_ReturnsNull_WhenFileNull()
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockFileService = new Mock<IFileUploadService>();
            var logger = new NullLogger<ProductManagementService>();

            var service = new ProductManagementService(mockUnitOfWork.Object, mockFileService.Object, logger);

            var result = await service.ReadFormFileAsBytesAsync(null);

            Assert.Null(result);
        }
    }
}
