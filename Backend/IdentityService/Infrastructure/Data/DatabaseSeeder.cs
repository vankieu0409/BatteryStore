using IdentityService.Domain.Entities;
using IdentityService.Domain.ValueObjects;
using IdentityService.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Tạo cơ sở dữ liệu nếu chưa tồn tại
            await _context.Database.MigrateAsync();

            // Kiểm tra xem đã có dữ liệu chưa
            if (!await _context.Roles.AnyAsync())
            {
                _logger.LogInformation("Bắt đầu seed dữ liệu ban đầu cho database...");
                await SeedRolesAsync();
                await SeedUsersAsync();
                _logger.LogInformation("Đã seed dữ liệu ban đầu thành công");
            }
            else
            {
                _logger.LogInformation("Database đã có dữ liệu, bỏ qua seed dữ liệu ban đầu");
            }
        }

        private async Task SeedRolesAsync()
        {
            _logger.LogInformation("Đang thêm các Role mặc định...");

            var roles = new List<Role>
            {
                Role.Create("Admin"),
                Role.Create("User"),
                Role.Create("Manager")
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã thêm {Count} Role mặc định", roles.Count);
        }

        private async Task SeedUsersAsync()
        {
            _logger.LogInformation("Đang thêm các User mặc định...");

            // Tạo Admin
            var adminEmail = Email.Create("admin@batteryshop.com");
            var adminPasswordHash = _passwordHasher.HashPassword("Admin@123");
            var admin = User.Create("admin", adminEmail, adminPasswordHash);

            // Tạo User thường
            var userEmail = Email.Create("user@batteryshop.com");
            var userPasswordHash = _passwordHasher.HashPassword("User@123");
            var user = User.Create("user", userEmail, userPasswordHash);

            // Thêm user vào database
            await _context.Users.AddRangeAsync(new[] { admin, user });
            await _context.SaveChangesAsync();

            // Gán roles cho users
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (adminRole != null)
            {
                admin.AddRole(adminRole);
            }

            if (userRole != null)
            {
                user.AddRole(userRole);
                admin.AddRole(userRole); // Admin cũng có quyền User
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã thêm các User mặc định");
        }
    }
}
