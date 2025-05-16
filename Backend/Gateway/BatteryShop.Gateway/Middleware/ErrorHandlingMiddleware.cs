using System.Net;
using System.Text.Json;

namespace BatteryShop.Gateway.Middleware;

/// <summary>
/// Middleware x? l� l?i chung cho Gateway
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
        _logger.LogError(exception, "L?i kh�ng x? l� ???c: {ErrorMessage}", exception.Message);

        var code = HttpStatusCode.InternalServerError; // 500 n?u kh�ng x�c ??nh ???c lo?i l?i
        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "?� x?y ra l?i trong qu� tr�nh x? l� y�u c?u.",
            Status = (int)code
        };

        // X? l� c�c lo?i exception c? th?
        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                errorResponse.Title = "Kh�ng c� quy?n truy c?p.";
                errorResponse.Status = (int)code;
                break;

            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                errorResponse.Title = "Kh�ng t�m th?y t�i nguy�n y�u c?u.";
                errorResponse.Status = (int)code;
                break;

            case ApplicationException when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase):
                code = HttpStatusCode.GatewayTimeout;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
                errorResponse.Title = "Y�u c?u qu� th?i gian x? l�.";
                errorResponse.Status = (int)code;
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Y�u c?u kh�ng h?p l?.";
                errorResponse.Status = (int)code;
                break;
        }

        // Th�m chi ti?t l?i trong m�i tr??ng ph�t tri?n
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
/// Extension method ?? th�m ErrorHandlingMiddleware v�o pipeline
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}