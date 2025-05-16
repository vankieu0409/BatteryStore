using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BatteryShop.Logging;

/// <summary>
/// Extension methods ?? ??ng k� c�c d?ch v? logging v�o container DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Th�m HttpClientLoggingHandler v�o container DI
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configureOptions">T�y ch?n c?u h�nh (optional)</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddHttpClientLogging(this IServiceCollection services, 
        Action<HttpClientLoggingOptions>? configureOptions = null)
    {
        // ??ng k� HttpClientLoggingOptions
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.TryAddSingleton(new HttpClientLoggingOptions());
        }
        
        // ??ng k� HttpClientLoggingHandler
        services.TryAddTransient<HttpClientLoggingHandler>();
        
        return services;
    }
    
    /// <summary>
    /// Th�m LoggingBehavior v�o container DI ?? s? d?ng v?i MediatR
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(LoggingBehavior<,>));
        return services;
    }
    
    /// <summary>
    /// C?u h�nh HttpClient v?i HttpClientLoggingHandler
    /// </summary>
    /// <param name="builder">IHttpClientBuilder</param>
    /// <returns>IHttpClientBuilder</returns>
    public static IHttpClientBuilder AddLogging(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<HttpClientLoggingHandler>();
    }
}