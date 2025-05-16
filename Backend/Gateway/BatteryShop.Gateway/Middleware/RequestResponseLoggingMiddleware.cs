using System.Diagnostics;
using System.Text;

namespace BatteryShop.Gateway.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {        // Đo thời gian xử lý request
        var stopwatch = Stopwatch.StartNew();
        
        // Ghi log thông tin request
        var requestBody = await GetRequestBodyAsync(context.Request);
        var logContext = new
        {
            RequestTime = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
            RequestMethod = context.Request.Method,
            RequestPath = context.Request.Path,
            RequestQuery = context.Request.QueryString.ToString(),
            RequestHeaders = GetFilteredHeaders(context.Request.Headers),
            RequestBody = requestBody,
            ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers.UserAgent.ToString()
        };

        _logger.LogInformation("HTTP Request: {RequestMethod} {RequestPath}{RequestQuery}", 
            context.Request.Method, context.Request.Path, context.Request.QueryString.ToString());        try
        {
            // Cho phép đọc lại response body bằng cách thay đổi response stream
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Tiếp tục pipeline
            await _next(context);
            
            // Ghi log thông tin response
            var responseBodyText = await GetResponseBodyAsync(context.Response);
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            var responseLog = new
            {
                ResponseTime = DateTime.UtcNow,
                ElapsedMilliseconds = elapsedMs,
                StatusCode = context.Response.StatusCode,
                ResponseHeaders = GetFilteredHeaders(context.Response.Headers),
                ResponseBody = responseBodyText
            };            _logger.LogInformation("HTTP Response {StatusCode} completed in {ElapsedMilliseconds}ms", 
                context.Response.StatusCode, elapsedMs);

            // Copy response body trả lại stream gốc để client nhận được
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            throw; // Ném lại exception để middleware xử lý lỗi có thể bắt được
        }
    }    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        // Không ghi log body cho các loại nội dung nhạy cảm hoặc lớn
        if (!IsTextBasedContentType(request.ContentType) || request.ContentLength > 10240)
            return "[Body not logged]";

        request.EnableBuffering();
        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        
        // Reset position để có thể đọc lại sau này
        request.Body.Seek(0, SeekOrigin.Begin);
        
        // Lọc thông tin nhạy cảm
        body = FilterSensitiveData(body);
        
        return body;
    }    private async Task<string> GetResponseBodyAsync(HttpResponse response)
    {
        // Không ghi log body cho các loại nội dung nhạy cảm hoặc lớn
        if (!IsTextBasedContentType(response.ContentType) || response.ContentLength > 10240)
            return "[Body not logged]";
            
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        
        // Lọc thông tin nhạy cảm
        text = FilterSensitiveData(text);
        
        return text;
    }

    private bool IsTextBasedContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        var mediaType = contentType.Split(';').First().ToLower().Trim();
        return mediaType.StartsWith("text/") || 
               mediaType == "application/json" ||
               mediaType == "application/xml" ||
               mediaType == "application/javascript";
    }    private IDictionary<string, string> GetFilteredHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>();
        
        foreach (var header in headers)
        {
            // Không ghi log một số header nhạy cảm
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                result[header.Key] = "[Redacted]";
            }
            else
            {
                result[header.Key] = header.Value.ToString();
            }
        }
        
        return result;
    }    private string FilterSensitiveData(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // Lọc các thông tin nhạy cảm (ví dụ: password, token)
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            "\"password\"\\s*:\\s*\"[^\"]*\"", 
            "\"password\":\"[REDACTED]\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            "\"token\"\\s*:\\s*\"[^\"]*\"", 
            "\"token\":\"[REDACTED]\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return content;
    }
}
