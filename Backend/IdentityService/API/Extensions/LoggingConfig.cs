using BatteryShop.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace IdentityService.API.Extensions;

public static class LoggingConfig
{
    public static IHostBuilder ConfigureEnvironmentSpecificLogging(this IHostBuilder host, IConfiguration configuration, IHostEnvironment environment)
    {
        host.AddBatteryShopLogging("IdentityService");
        
        if (environment.IsProduction())
        {
            // Trong môi trường production, thêm cấu hình Elasticsearch nếu có
            string elasticsearchUrl = configuration["ElasticsearchSettings:Uri"];
            if (!string.IsNullOrEmpty(elasticsearchUrl))
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
                    {
                        IndexFormat = $"identityservice-logs-{environment.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                        AutoRegisterTemplate = true,
                        NumberOfShards = 2,
                        NumberOfReplicas = 1
                    })
                    .CreateLogger();
            }
        }
        
        return host;
    }
}
