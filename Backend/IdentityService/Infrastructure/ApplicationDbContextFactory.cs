using System.Threading.Channels;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Build cấu hình từ appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // thư mục hiện tại khi chạy CLI
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityDb");
        optionsBuilder.UseSqlServer(connectionString);
        // Có thể hardcode hoặc đọc từ file nếu thích
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.LogTo(Console.WriteLine);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}