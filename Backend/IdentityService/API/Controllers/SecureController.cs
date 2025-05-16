using BatteryShop.Logging;
using IdentityService.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Yêu cầu xác thực JWT để truy cập các endpoint trong controller này
public class SecureController : ControllerBase
{
    private readonly ILogger<SecureController> _logger;
    
    public SecureController(ILogger<SecureController> logger)
    {
        _logger = logger;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        return await LogHelper.LogPerformanceAsync<SecureController, IActionResult>(_logger, "Secure Test Endpoint", async () =>
        {
            // Sử dụng extension methods để lấy thông tin người dùng
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var email = User.GetEmail();
            
            LogHelper.WithContext("UserAccess", new { UserId = userId, Username = username }, () =>
            {
                _logger.LogInformation("Người dùng đã truy cập endpoint bảo mật");
            });

            return Ok(new
            {
                Message = "Bạn đã xác thực thành công!",
                UserId = userId,
                Username = username,
                Email = email
            });
        });
    }
      [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")] // Chỉ cho phép người dùng có role Admin
    public async Task<IActionResult> AdminOnly()
    {
        return await LogHelper.LogPerformanceAsync<SecureController, IActionResult>(_logger, "Admin Only Endpoint", async () =>
        {
            var userId = User.GetUserId();
            var username = User.GetUsername();
            
            LogHelper.WithContext("AdminAccess", new { UserId = userId, Username = username }, () =>
            {
                _logger.LogInformation("Admin đã truy cập tài nguyên hạn chế");
            });
            
            return Ok(new
            {
                Message = "Bạn đã truy cập tài nguyên chỉ dành cho Admin!",
                UserId = userId
            });
        });
    }
}