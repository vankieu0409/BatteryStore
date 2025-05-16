using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace BatteryShop.Logging;

/// <summary>
/// DelegatingHandler ?? ghi log các HTTP request và response
/// </summary>
public class HttpClientLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggingHandler> _logger;
    private readonly HttpClientLoggingOptions _options;

    public HttpClientLoggingHandler(ILogger<HttpClientLoggingHandler> logger, HttpClientLoggingOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new HttpClientLoggingOptions();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(_options.LogLevel))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequest(request, requestId);

        try
        {
            // Send the request to the next handler
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            // Log response
            await LogResponse(response, requestId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Log(_options.LogLevel, ex,
                "[{RequestId}] HTTP Request failed after {ElapsedMilliseconds}ms: {ErrorMessage}",
                requestId, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    private async Task LogRequest(HttpRequestMessage request, string requestId)
    {
        var message = $"[{requestId}] HTTP Request: {request.Method} {request.RequestUri}";
        var requestDetail = new StringBuilder(message);

        if (_options.IncludeHeaders)
        {
            requestDetail.AppendLine();
            requestDetail.AppendLine("Headers:");
            foreach (var header in request.Headers)
            {
                var headerValues = string.Join(", ", header.Value);
                // L?c các header nh?y c?m
                var headerValue = _options.SensitiveHeaders.Contains(header.Key) ? "[REDACTED]" : headerValues;
                requestDetail.AppendLine($"  {header.Key}: {headerValue}");
            }
        }

        if (_options.IncludeBody && request.Content != null)
        {
            string content = await request.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Length < _options.MaxBodyLength)
            {
                var sanitizedContent = SanitizeContent(content);
                requestDetail.AppendLine();
                requestDetail.AppendLine("Body:");
                requestDetail.AppendLine(sanitizedContent);
            }
        }

        _logger.Log(_options.LogLevel, requestDetail.ToString());
    }

    private async Task LogResponse(HttpResponseMessage response, string requestId, long elapsedMilliseconds)
    {
        var message = $"[{requestId}] HTTP Response: {(int)response.StatusCode} {response.StatusCode} (completed in {elapsedMilliseconds}ms)";
        var responseDetail = new StringBuilder(message);

        if (_options.IncludeHeaders)
        {
            responseDetail.AppendLine();
            responseDetail.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                var headerValues = string.Join(", ", header.Value);
                // L?c các header nh?y c?m
                var headerValue = _options.SensitiveHeaders.Contains(header.Key) ? "[REDACTED]" : headerValues;
                responseDetail.AppendLine($"  {header.Key}: {headerValue}");
            }
        }

        if (_options.IncludeBody && response.Content != null)
        {
            string content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Length < _options.MaxBodyLength)
            {
                var sanitizedContent = SanitizeContent(content);
                responseDetail.AppendLine();
                responseDetail.AppendLine("Body:");
                responseDetail.AppendLine(sanitizedContent);
            }
        }

        _logger.Log(_options.LogLevel, responseDetail.ToString());
    }

    private string SanitizeContent(string content)
    {
        // Tr??ng h?p ??c bi?t cho JSON
        if (content.StartsWith("{") || content.StartsWith("["))
        {
            foreach (var pattern in _options.SensitiveJsonPatterns)
            {
                // Simple regex to redact sensitive values
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    pattern,
                    m => $"{m.Groups[1].Value}\"[REDACTED]\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }
        return content;
    }
}

/// <summary>
/// Tùy ch?n cho HttpClientLoggingHandler
/// </summary>
public class HttpClientLoggingOptions
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeBody { get; set; } = true;
    public int MaxBodyLength { get; set; } = 10240; // 10KB
    public HashSet<string> SensitiveHeaders { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "Api-Key"
    };
    public List<string> SensitiveJsonPatterns { get; } = new()
    {
        "(\"password\"\\s*:\\s*)\"[^\"]*\"",
        "(\"token\"\\s*:\\s*)\"[^\"]*\"",
        "(\"api_?key\"\\s*:\\s*)\"[^\"]*\"",
        "(\"secret\"\\s*:\\s*)\"[^\"]*\""
    };
}