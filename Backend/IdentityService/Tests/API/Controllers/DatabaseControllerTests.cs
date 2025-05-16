using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using IdentityService.API.Controllers;
using IdentityService.Infrastructure.Data;
using System.Threading.Tasks;
using Xunit;

namespace IdentityService.Tests.API.Controllers
{
    public class DatabaseControllerTests
    {
        private readonly Mock<DatabaseSeeder> _mockSeeder;
        private readonly Mock<ILogger<DatabaseController>> _mockLogger;
        private readonly DatabaseController _controller;

        public DatabaseControllerTests()
        {
            _mockSeeder = new Mock<DatabaseSeeder>(null, null, null);
            _mockLogger = new Mock<ILogger<DatabaseController>>();
            _controller = new DatabaseController(_mockSeeder.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SeedDatabaseAnonymous_WithValidKey_ReturnsOk()
        {
            // Arrange
            var apiKey = "development_seed_key_123";
            
            // Act
            var result = await _controller.SeedDatabaseAnonymous(apiKey);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockSeeder.Verify(s => s.SeedAsync(), Times.Once);
        }
        
        [Fact]
        public async Task SeedDatabaseAnonymous_WithInvalidKey_ReturnsUnauthorized()
        {
            // Arrange
            var apiKey = "invalid_key";
            
            // Act
            var result = await _controller.SeedDatabaseAnonymous(apiKey);
            
            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            _mockSeeder.Verify(s => s.SeedAsync(), Times.Never);
        }
    }
}
