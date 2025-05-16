using Microsoft.Extensions.Hosting;
using BatteryShop.Logging;

namespace BatteryShop.Gateway.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder builder)
    {
        return builder.AddBatteryShopLogging(options =>
        {
            options.ApplicationName = "BatteryShop.Gateway";
            options.EnableConsoleLogging = true;
            options.EnableElasticsearchLogging = true;
        });
    }
}

// Extension method to make it easier to reference the BatteryShop.Logging method
public static class LoggingBuilderExtensions 
{
    public static IHostBuilder AddBatteryShopLogging(this IHostBuilder builder, Action<BatteryShopLoggingOptions> configureOptions)
    {
        return BatteryShop.Logging.LoggingExtensions.AddBatteryShopLogging(builder, configureOptions);
    }
}