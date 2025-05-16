using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BatteryShop.Gateway.Handlers;
using BatteryShop.Gateway.Extensions;
using BatteryShop.Gateway.Middleware;
using BatteryShop.Logging;

// Sử dụng Serilog logging
var builder = WebApplication.CreateBuilder(args);

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

// Đăng ký AuthenticationDelegatingHandler (vẫn đăng ký nhưng sẽ không sử dụng trực tiếp)
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

// Thêm YARP Reverse Proxy và cấu hình delegates
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddRateLimiting()
    .AddTransforms();

// Thêm CORS với chính sách mở rộng
builder.Services.AddCorsPolicy(builder.Configuration);

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

// Rate limiting
builder.Services.AddRateLimiter();

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

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình endpoint health check nâng cao
app.UseHealthChecksConfig();

// Custom endpoints
app.MapGet("/", () => "BatteryShop API Gateway")
   .WithName("Root");

// YARP Reverse Proxy
app.MapReverseProxy();

// Ghi log khi ứng dụng khởi động
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("BatteryShop Gateway started. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
