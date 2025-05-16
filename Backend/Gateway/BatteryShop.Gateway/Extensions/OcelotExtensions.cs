using System.Threading.RateLimiting;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;

namespace BatteryShop.Gateway.Extensions;

public static class OcelotExtensions
{    /// <summary>
    /// Cấu hình Rate Limiting cho Ocelot
    /// </summary>
    public static IServiceCollection AddOcelotRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            // Cấu hình rate limit mặc định
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
            
            // Cấu hình rate limit cụ thể cho identity
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

        return services;
    }

    /// <summary>
    /// Cấu hình thêm các tùy chọn cho Ocelot
    /// </summary>
    public static OcelotBuilder AddCustomOcelotOptions(this OcelotBuilder builder)
    {
        // Thêm các cấu hình tùy chọn ở đây
        return builder;
    }
}
