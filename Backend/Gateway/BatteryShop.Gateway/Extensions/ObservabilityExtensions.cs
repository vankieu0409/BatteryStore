using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace BatteryShop.Gateway.Extensions;

public static class ObservabilityExtensions
{
    private static readonly string ServiceName = "BatteryShop.Gateway";
    private static readonly string ServiceVersion = "1.0.0";
    
    // Activity source cho custom tracing
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        // C?u hình OpenTelemetry Tracing
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.body_size", request.ContentLength);
                        activity.SetTag("http.request.host", request.Host.ToString());
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.body_size", response.ContentLength);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("error.type", exception.GetType().Name);
                        activity.SetTag("error.message", exception.Message);
                    };
                })
                .AddSource(ActivitySource.Name)
                .AddConsoleExporter()
            );

        // C?u hình OpenTelemetry Metrics
        services.AddOpenTelemetry()
            .WithMetrics(builder => builder
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                // .AddProcessInstrumentation() - Không có s?n trong .NET 9
                .AddConsoleExporter()
            );

        // Thêm Health Checks c? b?n
        services.AddHealthChecks()
            .AddCheck("gateway_alive", () => HealthCheckResult.Healthy("Gateway is running"), 
                tags: new[] { "infrastructure" });

        // Ki?m tra các microservices
        foreach (var clusterConfig in configuration.GetSection("ReverseProxy:Clusters").GetChildren())
        {
            var destinations = clusterConfig.GetSection("Destinations").GetChildren();
            foreach (var destination in destinations)
            {
                var address = destination.GetSection("Address").Value;
                if (!string.IsNullOrEmpty(address))
                {
                    services.AddHealthChecks()
                        .AddUrlGroup(new Uri($"{address.TrimEnd('/')}/health"), 
                            name: $"{clusterConfig.Key}_health", 
                            tags: new[] { "services" });
                }
            }
        }
            
        return services;
    }

    public static IApplicationBuilder UseHealthChecksConfig(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecks("/health/infrastructure", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("infrastructure"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecks("/health/services", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("services"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}

// Custom health check ??? Gateway
public class ApiGatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiGatewayHealthCheck> _logger;

    public ApiGatewayHealthCheck(ILogger<ApiGatewayHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ki?m tra các tài nguyên c?n thi?t cho Gateway
            // Ví d?: b? nh?, k?t n?i ??n các service quan tr?ng
            
            return Task.FromResult(HealthCheckResult.Healthy("API Gateway is running normally"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API Gateway health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("API Gateway health check failed", ex));
        }
    }
}