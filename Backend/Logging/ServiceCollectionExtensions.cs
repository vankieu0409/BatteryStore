using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BatteryShop.Logging;

/// <summary>
/// Extension methods ?? ??ng ký các d?ch v? logging vào container DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Thêm HttpClientLoggingHandler vào container DI
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configureOptions">Tùy ch?n c?u hình (optional)</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddHttpClientLogging(this IServiceCollection services, 
        Action<HttpClientLoggingOptions>? configureOptions = null)
    {
        // ??ng ký HttpClientLoggingOptions
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.TryAddSingleton(new HttpClientLoggingOptions());
        }
        
        // ??ng ký HttpClientLoggingHandler
        services.TryAddTransient<HttpClientLoggingHandler>();
        
        return services;
    }
    
    /// <summary>
    /// Thêm LoggingBehavior vào container DI ?? s? d?ng v?i MediatR
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(LoggingBehavior<,>));
        return services;
    }
    
    /// <summary>
    /// C?u hình HttpClient v?i HttpClientLoggingHandler
    /// </summary>
    /// <param name="builder">IHttpClientBuilder</param>
    /// <returns>IHttpClientBuilder</returns>
    public static IHttpClientBuilder AddLogging(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<HttpClientLoggingHandler>();
    }
}