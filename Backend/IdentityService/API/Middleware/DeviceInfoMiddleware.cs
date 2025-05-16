using IdentityService.Application.DTOs;

namespace IdentityService.API.Middleware;

public class DeviceInfoMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }    public async Task InvokeAsync(HttpContext context)
    {
        // Đường dẫn API đăng nhập
        if (context.Request.Path.Value?.EndsWith("/login") == true && 
            context.Request.Method == "POST")
        {
            // Lưu stream ban đầu
            var originalBodyStream = context.Response.Body;            try
            {
                // Cho phép đọc request body nhiều lần
                context.Request.EnableBuffering();
                
                // Lấy thông tin thiết bị từ User-Agent
                var userAgent = context.Request.Headers.UserAgent.ToString();
                
                // Lấy địa chỉ IP
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                // Chuẩn bị thông tin thiết bị
                var deviceInfo = $"{userAgent} | IP: {ipAddress}";
                  // Lưu thông tin vào HttpContext để controller có thể sử dụng
                context.Items["DeviceInfo"] = deviceInfo;
                
                // Tiếp tục pipeline
                await _next(context);
            }
            finally
            {
                // Đảm bảo trả về stream ban đầu
                context.Response.Body = originalBodyStream;
            }
        }
        else
        {
            await _next(context);
        }
    }
}

public static class DeviceInfoMiddlewareExtensions
{
    public static IApplicationBuilder UseDeviceInfo(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DeviceInfoMiddleware>();
    }
}