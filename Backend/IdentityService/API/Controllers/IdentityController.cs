using BatteryShop.Logging;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{    
    private readonly IIdentityService _identityService;
    private readonly ILogger<IdentityController> _logger;
    
    public IdentityController(IIdentityService identityService, ILogger<IdentityController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }    
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(Application.DTOs.RegisterRequest request)
    {
        return await LogHelper.LogPerformanceAsync<IdentityController, IActionResult>(_logger, "Register Request", async () =>
        {
            LogHelper.WithContext("Username", request.UserName, () => 
            {
                _logger.LogInformation("Đang xử lý đăng ký người dùng mới thông qua IdentityService");
            });
            
            var result = await _identityService.RegisterAsync(request);
            
            if (result.Success)
            {
                _logger.LogInformation("Đăng ký thành công cho người dùng {Username}", request.UserName);
            }
            else
            {
                _logger.LogWarning("Đăng ký thất bại cho người dùng {Username}: {ErrorMessage}", 
                    request.UserName, result.Message);
            }
            
            return Ok(result);
        });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(Application.DTOs.LoginRequest request)
    {
        return await LogHelper.LogPerformanceAsync<IdentityController, IActionResult>(_logger, "User Login", async () =>
        {
            LogHelper.WithContext("Username", request.UserNameOrEmail, () => 
            {
                _logger.LogInformation("Đang xử lý đăng nhập người dùng");
            });
        
            // Lấy thông tin thiết bị từ middleware nếu có
            if (HttpContext.Items.TryGetValue("DeviceInfo", out var deviceInfo))
            {
                request.DeviceInfo = deviceInfo?.ToString();
                _logger.LogInformation("Thiết bị đăng nhập: {DeviceInfo}", deviceInfo);
            }
            
            var result = await _identityService.LoginAsync(request);
            if (result.Success)
            {
                _logger.LogInformation("Đăng nhập thành công cho người dùng {Username}", request.UserNameOrEmail);            
                // Set AccessToken và RefreshToken vào HttpOnly cookie
                Response.Cookies.Append("AccessToken", result.AccessToken ?? string.Empty, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
                Response.Cookies.Append("RefreshToken", result.RefreshToken ?? string.Empty, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
            else
            {
                _logger.LogWarning("Đăng nhập thất bại cho người dùng {Username}: {ErrorMessage}", 
                    request.UserNameOrEmail, result.Message);
            }
            return Ok(result);
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var result = await _identityService.LogoutAsync(request);
        if (result.Success)
        {
            // Xóa cookie khi đăng xuất thành công
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
        }
        return Ok(result);
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetProfile(string userId)
        => Ok(await _identityService.GetProfileAsync(userId));

    [HttpPut("profile/{userId}")]
    public async Task<IActionResult> UpdateProfile(string userId, UpdateUserProfileRequest request)
        => Ok(await _identityService.UpdateProfileAsync(userId, request));

    [HttpPost("change-password/{userId}")]
    public async Task<IActionResult> ChangePassword(string userId, ChangePasswordRequest request)
        => Ok(await _identityService.ChangePasswordAsync(userId, request));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(Application.DTOs.ForgotPasswordRequest request)
        => Ok(await _identityService.ForgotPasswordAsync(request));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(Application.DTOs.ResetPasswordRequest request)
        => Ok(await _identityService.ResetPasswordAsync(request));

    [HttpPost("2fa/setup")]
    public async Task<IActionResult> SetupTwoFactor(TwoFactorSetupRequest request)
        => Ok(await _identityService.SetupTwoFactorAsync(request));

    [HttpPost("2fa/verify")]
    public async Task<IActionResult> VerifyTwoFactor(TwoFactorVerifyRequest request)
        => Ok(await _identityService.VerifyTwoFactorAsync(request));

    [HttpPost("role")]
    public async Task<IActionResult> CreateRole(RoleRequest request)
        => Ok(await _identityService.CreateRoleAsync(request));

    [HttpPost("role/assign")]
    public async Task<IActionResult> AssignRole(AssignRoleRequest request)
        => Ok(await _identityService.AssignRoleAsync(request));

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _identityService.RefreshTokenAsync(request);
        if (result.Success)
        {
            // Set AccessToken và RefreshToken mới vào HttpOnly cookie
            Response.Cookies.Append("AccessToken", result.AccessToken ?? string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            Response.Cookies.Append("RefreshToken", result.RefreshToken ?? string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }
        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(EmailVerificationRequest request)
        => Ok(await _identityService.VerifyEmailAsync(request));

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone(PhoneVerificationRequest request)
        => Ok(await _identityService.VerifyPhoneAsync(request));

    [HttpGet("sessions/{userId}")]
    public async Task<IActionResult> GetSessions(string userId)
        => Ok(await _identityService.GetSessionsAsync(userId));

    [HttpPost("sessions/revoke/{userId}")]
    public async Task<IActionResult> RevokeSession(string userId, [FromBody] string refreshToken)
        => Ok(await _identityService.RevokeSessionAsync(userId, refreshToken));

    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin(ExternalLoginRequest request)
        => Ok(await _identityService.ExternalLoginAsync(request));
}
