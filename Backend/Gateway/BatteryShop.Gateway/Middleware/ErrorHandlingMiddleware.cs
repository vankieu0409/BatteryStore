using System.Net;
using System.Text.Json;

namespace BatteryShop.Gateway.Middleware;

/// <summary>
/// Middleware x? lý l?i chung cho Gateway
/// </summary>
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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway error: {ErrorMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "L?i không x? lý ???c: {ErrorMessage}", exception.Message);

        var code = HttpStatusCode.InternalServerError; // 500 n?u không xác ??nh ???c lo?i l?i
        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "?ã x?y ra l?i trong quá trình x? lý yêu c?u.",
            Status = (int)code
        };

        // X? lý các lo?i exception c? th?
        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                errorResponse.Title = "Không có quy?n truy c?p.";
                errorResponse.Status = (int)code;
                break;

            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                errorResponse.Title = "Không tìm th?y tài nguyên yêu c?u.";
                errorResponse.Status = (int)code;
                break;

            case ApplicationException when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase):
                code = HttpStatusCode.GatewayTimeout;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
                errorResponse.Title = "Yêu c?u quá th?i gian x? lý.";
                errorResponse.Status = (int)code;
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Yêu c?u không h?p l?.";
                errorResponse.Status = (int)code;
                break;
        }

        // Thêm chi ti?t l?i trong môi tr??ng phát tri?n
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

/// <summary>
/// Extension method ?? thêm ErrorHandlingMiddleware vào pipeline
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}