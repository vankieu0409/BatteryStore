using BatteryShop.Logging;
using FluentValidation;
using System.Net;
using System.Text.Json;
using IdentityService.Domain.Common;

namespace IdentityService.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Sử dụng LogHelper để đo thời gian xử lý request và ghi log nếu có lỗi
        try
        {
            // Thêm requestId vào LogContext
            var requestId = context.TraceIdentifier;
            string? userId = context.User?.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst("sub")?.Value 
                : null;

            await LogHelper.WithRequestContext(requestId, userId, async () =>
            {
                await _next(context);
                return true;
            });
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Lỗi không xử lý được: {ErrorMessage}", exception.Message);

        var code = HttpStatusCode.InternalServerError; // 500 nếu không xác định được loại lỗi
        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.",
            Status = (int)code
        };

        // Xử lý các loại exception cụ thể
        switch (exception)
        {
            case ValidationException validationEx:
                code = HttpStatusCode.BadRequest;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Một hoặc nhiều lỗi xác thực đã xảy ra.";
                errorResponse.Status = (int)code;
                errorResponse.Errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case DomainException domainEx:
                code = HttpStatusCode.BadRequest;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = domainEx.Message;
                errorResponse.Status = (int)code;
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                errorResponse.Title = "Không có quyền truy cập.";
                errorResponse.Status = (int)code;
                break;

            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                errorResponse.Title = "Không tìm thấy tài nguyên yêu cầu.";
                errorResponse.Status = (int)code;
                break;
        }

        // Thêm chi tiết lỗi trong môi trường phát triển
        if (_environment.IsDevelopment())
        {
            errorResponse.Detail = exception.ToString();
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)code;
        
        return context.Response.WriteAsync(result);
    }

    private class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
