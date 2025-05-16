using BatteryShop.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Extensions;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký DbContext
        services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Scoped);

        // Đăng ký repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();


        // Đăng ký domain services
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Đăng ký database seeder
        services.AddScoped<DatabaseSeeder>();

        // Đăng ký application services
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        // Đăng ký database seeder
        services.AddDatabaseSeeder();

        // Đăng ký HTTP client logging
        services.AddHttpClientLogging(options =>
        {
            options.IncludeHeaders = true;
            options.IncludeBody = true;
            options.SensitiveHeaders.Add("Authorization");
        });

        // Đăng ký named HttpClient với logging
        services.AddHttpClient("notification-service", client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:NotificationService"] ?? "http://localhost:5005");
        }).AddLogging(); // Thêm handler ghi log HTTP requests/responses

        return services;
    }
}
