using BatteryShop.Logging;
using IdentityService.API.Extensions;
using IdentityService.API.Middleware;
using IdentityService.Infrastructure.Extensions;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Cấu hình Serilog với các profile cụ thể theo môi trường
    builder.Host.ConfigureEnvironmentSpecificLogging(builder.Configuration, builder.Environment);
    
    // Sử dụng các extension methods để cấu hình services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddCorsPolicy(builder.Configuration);

    var app = builder.Build();

// Khởi tạo và seed database
await app.InitializeDatabaseAsync();

// Configure middleware pipeline
// Thêm middleware xử lý lỗi - phải đặt đầu tiên trong pipeline
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseDeviceInfo(); // Thêm middleware lấy thông tin thiết bị
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    // Ghi log lỗi khởi động ứng dụng
    Log.Fatal(ex, "Ứng dụng dừng do lỗi không xử lý được");
}
finally
{
    // Đảm bảo tất cả log được ghi và tài nguyên được giải phóng
    Log.CloseAndFlush();
}
