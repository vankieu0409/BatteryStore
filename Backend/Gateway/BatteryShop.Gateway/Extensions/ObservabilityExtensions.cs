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
            .AddEnvironmentVariableDetector();        // Cấu hình OpenTelemetry Tracing
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
            );        // Cấu hình OpenTelemetry Metrics
        services.AddOpenTelemetry()
            .WithMetrics(builder => builder
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                // .AddProcessInstrumentation() - Không có sẵn trong .NET 9
                .AddConsoleExporter()
            );        // Thêm Health Checks cơ bản
        services.AddHealthChecks()
            .AddCheck("gateway_alive", () => HealthCheckResult.Healthy("Gateway is running"), 
                tags: new[] { "infrastructure" });        // Kiểm tra các microservices
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
}

// Custom health check cho Gateway
public class ApiGatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiGatewayHealthCheck> _logger;

    public ApiGatewayHealthCheck(ILogger<ApiGatewayHealthCheck> logger)
    {
        _logger = logger;
    }    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Kiểm tra các tài nguyên cần thiết cho Gateway
            // Ví dụ: bộ nhớ, kết nối đến các service quan trọng
            
            return Task.FromResult(HealthCheckResult.Healthy("API Gateway is running normally"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API Gateway health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("API Gateway health check failed", ex));
        }
    }
}