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

            // C?u hình Swagger ?? s? d?ng JWT Bearer Authentication
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
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder.AllowAnyOrigin() //.WithOrigins(configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ??
                    // new[] { "http://localhost:3000", "https://localhost:3000" })
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                //.AllowCredentials();
            });
        });

        return services;
    }
}