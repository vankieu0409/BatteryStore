using IdentityService.Application;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace IdentityService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký API filters
        services.AddControllers();

        // Đăng ký Application và Infrastructure services
        
        services.AddInfrastructureServices(configuration);
        services.AddApplicationServices();
        return services;
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Đặt thành true trong môi trường production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidAudience = configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    configuration["JWT:SecretKey"] ?? "super_secret_key_123!@#_for_gateway_validation")),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    // Có thể thêm kiểm tra user tồn tại hoặc bị khoá ở đây
                    return Task.CompletedTask;
                }
            };
        });
    }

    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity Service API",
                Version = "v1",
                Description = "Identity Service API for Battery Shop"
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
    }

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", builder =>
            {
                builder.AllowAnyOrigin()
                    //.WithOrigins(configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? 
                    //            new[] { "http://localhost:3000", "https://localhost:3000" })
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                   // .AllowCredentials();
            });
        });
    }
}