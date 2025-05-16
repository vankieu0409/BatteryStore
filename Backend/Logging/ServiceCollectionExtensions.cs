using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BatteryShop.Logging;

/// <summary>
/// Extension methods để đăng ký các dịch vụ logging vào container DI
/// </summary>
public static class ServiceCollectionExtensions
{    /// <summary>
    /// Thêm HttpClientLoggingHandler vào container DI
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configureOptions">Tùy chọn cấu hình (optional)</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddHttpClientLogging(this IServiceCollection services, 
        Action<HttpClientLoggingOptions>? configureOptions = null)
    {
        // Đăng ký HttpClientLoggingOptions
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.TryAddSingleton(new HttpClientLoggingOptions());
        }
        
        // Đăng ký HttpClientLoggingHandler
        services.TryAddTransient<HttpClientLoggingHandler>();
        
        return services;
    }
      /// <summary>
    /// Thêm LoggingBehavior vào container DI để sử dụng với MediatR
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(LoggingBehavior<,>));
        return services;
    }
      /// <summary>
    /// Cấu hình HttpClient với HttpClientLoggingHandler
    /// </summary>
    /// <param name="builder">IHttpClientBuilder</param>
    /// <returns>IHttpClientBuilder</returns>
    public static IHttpClientBuilder AddLogging(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<HttpClientLoggingHandler>();
    }
}