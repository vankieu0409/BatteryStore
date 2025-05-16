using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BatteryShop.Gateway.Handlers;
using BatteryShop.Gateway.Extensions;
using BatteryShop.Gateway.Middleware;
using BatteryShop.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Ocelot.Provider.Polly;

// Sử dụng Serilog logging
var builder = WebApplication.CreateBuilder(args);

// Thêm cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Cấu hình logging bằng Serilog
builder.Host.AddSerilogLogging();

// Thêm Observability (OpenTelemetry)
builder.Services.AddObservability(builder.Configuration);

// Đăng ký các dịch vụ logging
builder.Services.AddHttpClientLogging(options => 
{
    options.IncludeHeaders = true;
    options.IncludeBody = true;
});

// Thêm HttpContextAccessor để truy cập HttpContext từ DelegatingHandler
builder.Services.AddHttpContextAccessor();

// Đăng ký AuthenticationDelegatingHandler
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

// Đăng ký HttpClient cho các services
builder.Services.AddHttpClients(builder.Configuration);

// Thêm Health checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("https://localhost:5001/health"), "Identity Service")
    .AddUrlGroup(new Uri("https://localhost:5002/health"), "Product Service");

// Thêm Ocelot và các tính năng bổ sung
builder.Services.AddOcelot(builder.Configuration)
    .AddPolly()
    .AddCacheManager(x => 
    {
        x.WithDictionaryHandle();
    });

// Thêm CORS với chính sách mở rộng
builder.Services.AddCorsPolicy(builder.Configuration);

// Thêm Rate Limiting
builder.Services.AddOcelotRateLimiting();

// Xác thực JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JWT:SecretKey"] ?? "super_secret_key_123!@#_for_gateway_validation")),
        ClockSkew = TimeSpan.Zero
    };
    
    // Xử lý khi token hợp lệ
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            // Sử dụng LogHelper để ghi log với context
            LogHelper.WithContext("UserId", context.Principal?.FindFirst("sub")?.Value, () =>
            {
                logger.LogInformation("User authenticated successfully");
            });
            
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// Thêm Swagger nâng cao
builder.Services.AddSwaggerDocumentation();

// Rate limiting đã được thiết lập trong AddOcelotRateLimiting()

var app = builder.Build();

// Cấu hình pipeline
app.UseErrorHandling(); // Custom error handling middleware

// Middleware cho logging requests/responses
app.UseRequestResponseLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sử dụng CORS với chính sách mở rộng
app.UseCors("DefaultPolicy");

// Rate limiting middleware
app.UseRateLimiter();

// Middleware xác thực token tùy chỉnh
app.UseAuthenticationMiddleware();

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình endpoint health check nâng cao
app.UseHealthChecksConfig();

// Custom endpoints
app.MapGet("/", () => "BatteryShop API Gateway")
   .WithName("Root");

// Sử dụng Ocelot middleware
await app.UseOcelot();

// Ghi log khi ứng dụng khởi động
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("BatteryShop Gateway started. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
