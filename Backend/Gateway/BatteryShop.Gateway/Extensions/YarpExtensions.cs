using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

namespace BatteryShop.Gateway.Extensions;

public static class YarpExtensions
{
    /// <summary>
    /// C?u hình Rate Limiting cho YARP ReverseProxy
    /// </summary>
    public static IReverseProxyBuilder AddRateLimiting(this IReverseProxyBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            // C?u hình rate limit m?c ??nh
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
            
            // C?u hình rate limit c? th? cho identity-route
            options.AddPolicy("identity", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });

        return builder;
    }

    /// <summary>
    /// C?u hình các transforms cho request và response
    /// </summary>
    public static IReverseProxyBuilder AddTransforms(this IReverseProxyBuilder builder)
    {
        builder.AddTransforms(transformBuilder =>
        {
            // Thêm header ?? xác ??nh ngu?n request là t? API Gateway
            transformBuilder.AddRequestTransform(context =>
            {
                context.ProxyRequest.Headers.Add("X-Forwarded-By", "BatteryShop-Gateway");
                return ValueTask.CompletedTask;
            });
            
            // Logging request
            transformBuilder.AddRequestTransform(context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Gateway forwarding request {Method} {Path} to {Destination}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    context.DestinationPrefix);
                return ValueTask.CompletedTask;
            });

            // Thêm AuthenticationDelegatingHandler vào pipeline transform
            transformBuilder.AddRequestTransform(context =>
            {
                // Chuy?n ti?p token t? client request ??n microservices
                if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    context.ProxyRequest.Headers.Add("Authorization", authHeader.ToArray());
                }
                // N?u không có Authorization header, ki?m tra Cookie
                else if (context.HttpContext.Request.Cookies.TryGetValue("AccessToken", out var token))
                {
                    context.ProxyRequest.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                return ValueTask.CompletedTask;
            });
        });

        return builder;
    }
}