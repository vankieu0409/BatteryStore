using IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                var seeder = services.GetRequiredService<DatabaseSeeder>();

                logger.LogInformation("Đang khởi tạo cơ sở dữ liệu...");
                await seeder.SeedAsync();
                logger.LogInformation("Khởi tạo cơ sở dữ liệu thành công");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Xảy ra lỗi khi khởi tạo cơ sở dữ liệu");
                throw;
            }
        }

        public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services)
        {
            services.AddTransient<DatabaseSeeder>();
            return services;
        }
    }
}
