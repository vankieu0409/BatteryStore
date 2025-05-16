using IdentityService.Application.DTOs;

namespace IdentityService.API.Middleware;

public class DeviceInfoMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ???ng d?n API ??ng nh?p
        if (context.Request.Path.Value?.EndsWith("/login") == true && 
            context.Request.Method == "POST")
        {
            // L?u stream ban ??u
            var originalBodyStream = context.Response.Body;

            try
            {
                // Cho phép ??c request body nhi?u l?n
                context.Request.EnableBuffering();
                
                // L?y thông tin thi?t b? t? User-Agent
                var userAgent = context.Request.Headers.UserAgent.ToString();
                
                // L?y ??a ch? IP
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                // Chu?n b? thông tin thi?t b?
                var deviceInfo = $"{userAgent} | IP: {ipAddress}";
                
                // L?u thông tin vào HttpContext ?? controller có th? s? d?ng
                context.Items["DeviceInfo"] = deviceInfo;
                
                // Ti?p t?c pipeline
                await _next(context);
            }
            finally
            {
                // ??m b?o tr? v? stream ban ??u
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