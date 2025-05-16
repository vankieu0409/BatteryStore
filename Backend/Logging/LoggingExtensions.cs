

using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace BatteryShop.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Cấu hình Serilog cho tất cả các service trong hệ thống BatteryShop
    /// </summary>
    /// <param name="builder">IHostBuilder</param>
    /// <param name="applicationName">Tên của ứng dụng/service</param>
    /// <returns>IHostBuilder với cấu hình Serilog</returns>
    public static IHostBuilder AddBatteryShopLogging(this IHostBuilder builder, string applicationName)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            var env = context.HostingEnvironment;
            var elasticUri = context.Configuration["ElasticSearch:Uri"] ?? "http://localhost:9200";

            var logConfig = configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("Environment", env.EnvironmentName)
                .Enrich.FromLogContext();

            // Ghi log ra console với định dạng ngắn gọn
            logConfig.WriteTo.Console(outputTemplate: 
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Cấu hình Elasticsearch sink nếu được cung cấp URI
            if (!string.IsNullOrEmpty(elasticUri))
            {
                logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    IndexFormat = $"{applicationName.ToLower().Replace(".", "-")}-{env.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                    AutoRegisterTemplate = true,
                    NumberOfReplicas = 1,
                    NumberOfShards = 2,
                    FailureCallback = (e, _) => Console.WriteLine($"Unable to submit log event to Elasticsearch: {e}"),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    MinimumLogEventLevel = LogEventLevel.Information
                });
            }

            // Đọc cấu hình bổ sung từ appsettings.json
            logConfig.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services);
        });
    }

    /// <summary>
    /// Cấu hình Serilog cho tất cả các service, với tùy chọn nâng cao
    /// </summary>
    /// <param name="builder">IHostBuilder</param>
    /// <param name="configureOptions">Hàm cấu hình tùy chọn</param>
    /// <returns>IHostBuilder với cấu hình Serilog</returns>
    public static IHostBuilder AddBatteryShopLogging(this IHostBuilder builder, 
        Action<BatteryShopLoggingOptions> configureOptions)
    {
        var options = new BatteryShopLoggingOptions();
        configureOptions(options);

        return builder.UseSerilog((context, services, configuration) =>
        {
            var env = context.HostingEnvironment;
            var elasticUri = context.Configuration["ElasticSearch:Uri"] ?? options.ElasticsearchUri;

            var logConfig = configuration
                .MinimumLevel.Is(options.MinimumLevel)
                .Enrich.WithProperty("ApplicationName", options.ApplicationName)
                .Enrich.WithProperty("Environment", env.EnvironmentName)
                .Enrich.FromLogContext();

            // Thêm các enrichers
            foreach (var enricher in options.Enrichers)
            {
                logConfig = enricher(logConfig);
            }

            // Ghi log ra console nếu được cấu hình
            if (options.EnableConsoleLogging)
            {
                logConfig.WriteTo.Console(
                    restrictedToMinimumLevel: options.ConsoleMinimumLevel,
                    outputTemplate: options.ConsoleOutputTemplate);
            }

            // Cấu hình Elasticsearch sink nếu được bật
            if (options.EnableElasticsearchLogging && !string.IsNullOrEmpty(elasticUri))
            {
                logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    IndexFormat = options.ElasticsearchIndexFormat ?? 
                                  $"{options.ApplicationName.ToLower().Replace(".", "-")}-{env.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                    AutoRegisterTemplate = options.ElasticsearchAutoRegisterTemplate,
                    NumberOfReplicas = options.ElasticsearchNumberOfReplicas,
                    NumberOfShards = options.ElasticsearchNumberOfShards,
                    FailureCallback = (e, _) => Console.WriteLine($"Unable to submit log event to Elasticsearch: {e}"),
                    EmitEventFailure = options.ElasticsearchEmitEventFailure,
                    MinimumLogEventLevel = options.ElasticsearchMinimumLevel
                });
            }

            // Đọc cấu hình bổ sung từ appsettings.json nếu được bật
            if (options.ReadFromConfiguration)
            {
                logConfig.ReadFrom.Configuration(context.Configuration);
            }
            
            if (options.ReadFromServices)
            {
                logConfig.ReadFrom.Services(services);
            }
        });
    }
}

/// <summary>
/// Lớp tùy chọn cho việc cấu hình logging
/// </summary>
public class BatteryShopLoggingOptions
{
    public string ApplicationName { get; set; } = "BatteryShopService";
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    
    // Console logging options
    public bool EnableConsoleLogging { get; set; } = true;
    public LogEventLevel ConsoleMinimumLevel { get; set; } = LogEventLevel.Information;
    public string ConsoleOutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    
    // Elasticsearch options
    public bool EnableElasticsearchLogging { get; set; } = true;
    public string? ElasticsearchUri { get; set; } = "http://localhost:9200";
    public string? ElasticsearchIndexFormat { get; set; } = null;
    public bool ElasticsearchAutoRegisterTemplate { get; set; } = true;
    public int ElasticsearchNumberOfReplicas { get; set; } = 1;
    public int ElasticsearchNumberOfShards { get; set; } = 2;
    public EmitEventFailureHandling ElasticsearchEmitEventFailure { get; set; } = 
        EmitEventFailureHandling.WriteToSelfLog | 
        EmitEventFailureHandling.WriteToFailureSink | 
        EmitEventFailureHandling.RaiseCallback;
    public LogEventLevel ElasticsearchMinimumLevel { get; set; } = LogEventLevel.Information;
    
    // Configuration options
    public bool ReadFromConfiguration { get; set; } = true;
    public bool ReadFromServices { get; set; } = true;
    
    // List of custom enrichers
    public List<Func<LoggerConfiguration, LoggerConfiguration>> Enrichers { get; } = new();
    
    // Thêm custom enricher
    public void AddEnricher(Func<LoggerConfiguration, LoggerConfiguration> enricher)
    {
        Enrichers.Add(enricher);
    }
}