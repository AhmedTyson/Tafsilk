using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Controllers;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.TailorManagement;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Interfaces;
using Moq;
using System.Collections.Generic;

namespace TafsilkPlatform.Web.Tests.Integration
{
    public class ControllerFallbackTests
    {
        private AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task TailorManageProducts_ReturnsFallbackModel_WhenServiceReturnsNull()
        {
            // Arrange
            var db = CreateInMemoryDb();
            var logger = new NullLogger<TafsilkPlatform.Web.Controllers.TailorManagementController>();

            var mockProductService = new Mock<IProductManagementService>();
            mockProductService.Setup(s => s.GetTailorProductsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ManageProductsViewModel?)null);

            var mockPortfolioService = new Mock<IPortfolioService>();

            var controller = new TafsilkPlatform.Web.Controllers.TailorManagementController(
                db,
                new NullLogger<TafsilkPlatform.Web.Controllers.TailorManagementController>(),
                new NullFileUploadService(),
                mockProductService.Object,
                mockPortfolioService.Object);

            // Seed a tailor profile
            var tailor = new TailorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FullName = "Test" };
            db.TailorProfiles.Add(tailor);
            await db.SaveChangesAsync();

            // Fake authentication by setting a claim via controller's User is harder in unit test; use GetTailorProfileAsync override approach
            // Act
            var result = await controller.ManageProducts();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ManageProductsViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(tailor.Id, model.TailorId);
            Assert.Empty(model.Products);
        }

        [Fact]
        public async Task TailorManagePortfolio_ReturnsFallbackModel_WhenServiceReturnsNull()
        {
            // Arrange
            var db = CreateInMemoryDb();
            var mockPortfolioService = new Mock<IPortfolioService>();
            mockPortfolioService.Setup(s => s.GetTailorPortfolioAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ManagePortfolioViewModel?)null);

            var mockProductService = new Mock<IProductManagementService>();

            var controller = new TafsilkPlatform.Web.Controllers.TailorManagementController(
                db,
                new NullLogger<TafsilkPlatform.Web.Controllers.TailorManagementController>(),
                new NullFileUploadService(),
                mockProductService.Object,
                mockPortfolioService.Object);

            var tailor = new TailorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FullName = "Test" };
            db.TailorProfiles.Add(tailor);
            await db.SaveChangesAsync();

            var result = await controller.ManagePortfolio();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ManagePortfolioViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(tailor.Id, model.TailorId);
            Assert.Empty(model.Images);
        }

        // Minimal file upload service stub for tests
        private class NullFileUploadService : IFileUploadService
        {
            public bool IsValidImage(Microsoft.AspNetCore.Http.IFormFile file) => true;
            public long GetMaxFileSizeInBytes() => 5 * 1024 * 1024;
            public Task<string> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile file, string folder) => Task.FromResult(string.Empty);
            public Task DeleteFileAsync(string path) => Task.CompletedTask;
        }
    }
}
