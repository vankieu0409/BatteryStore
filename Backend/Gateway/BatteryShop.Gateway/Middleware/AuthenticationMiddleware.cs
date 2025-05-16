using BatteryShop.Logging;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace BatteryShop.Gateway.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Kiểm tra xem request có chứa token không
        string? token = null;
        
        // Kiểm tra token trong header Authorization
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authHeaderValue = authHeader.ToString();
            if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeaderValue["Bearer ".Length..].Trim();
            }
        }
        // Nếu không có header, kiểm tra trong cookie
        else if (context.Request.Cookies.TryGetValue("AccessToken", out var cookieToken))
        {
            token = cookieToken;
        }

        // Nếu có token, ghi log và thêm vào ngữ cảnh
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Đơn giản hóa việc xử lý token - chỉ đọc claims
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                    if (userIdClaim != null)
                    {
                        // Thêm thông tin người dùng vào log context
                        LogHelper.WithContext("UserId", userIdClaim, () =>
                        {
                            _logger.LogInformation("Xác thực token thành công, tiếp tục xử lý request");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi xử lý token");
            }
        }

        await _next(context);
    }
}
