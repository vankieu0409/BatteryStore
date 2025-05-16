using IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseSeeder _seeder;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(
            DatabaseSeeder seeder,
            ILogger<DatabaseController> logger)
        {
            _seeder = seeder;
            _logger = logger;
        }

        [HttpPost("seed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                _logger.LogInformation("Yêu cầu seed database từ API");
                await _seeder.SeedAsync();
                return Ok(new { message = "Database đã được seed thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi seed database từ API");
                return StatusCode(500, new { message = "Lỗi khi seed database", error = ex.Message });
            }
        }

        [HttpPost("seed/anonymous")]
        public async Task<IActionResult> SeedDatabaseAnonymous([FromQuery] string apiKey)
        {
            // Kiểm tra apiKey (chỉ dùng trong môi trường development)
            if (apiKey != "development_seed_key_123")
            {
                _logger.LogWarning("Yêu cầu seed database từ API không hợp lệ do apiKey không chính xác");
                return Unauthorized(new { message = "API key không hợp lệ" });
            }

            try
            {
                _logger.LogInformation("Yêu cầu seed database từ API (anonymous)");
                await _seeder.SeedAsync();
                return Ok(new { message = "Database đã được seed thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi seed database từ API");
                return StatusCode(500, new { message = "Lỗi khi seed database", error = ex.Message });
            }
        }
    }
}
