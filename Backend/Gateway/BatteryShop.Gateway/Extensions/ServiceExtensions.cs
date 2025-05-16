using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace BatteryShop.Gateway.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BatteryShop Gateway API",
                Version = "v1",
                Description = "API Gateway for Battery Shop Microservices",
                Contact = new OpenApiContact
                {
                    Name = "BatteryShop Team",
                    Email = "support@batteryshop.example.com"
                }
            });

            // Cấu hình Swagger để sử dụng JWT Bearer Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? 
            new[] { "http://localhost:3000", "https://localhost:3000" };

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

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

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký HttpClient cho IdentityService
        services.AddHttpClient("identity-service", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        // Đăng ký HttpClient cho ProductService
        services.AddHttpClient("product-service", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5002/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        // Các HttpClient khác có thể được thêm vào tương tự
        
        return services;
    }
}