using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BatteryShop.Logging;

/// <summary>
/// L?p h? tr? ghi log các request và response c?a các service
/// Có th? s? d?ng v?i MediatR pipeline behavior
/// </summary>
/// <typeparam name="TRequest">Lo?i request</typeparam>
/// <typeparam name="TResponse">Lo?i response</typeparam>
public class LoggingBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// X? lý request và ghi log thông tin
    /// </summary>
    /// <param name="request">Request c?n x? lý</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="next">Hàm x? lý ti?p theo trong pipeline</param>
    /// <returns>Response t? handler</returns>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, 
        Func<TRequest, CancellationToken, Task<TResponse>> next)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("[{RequestId}] Handling request {RequestName}: {@Request}", 
            requestId, requestName, SanitizeRequest(request));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(request, cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("[{RequestId}] Successfully handled request {RequestName} in {ElapsedTime}ms",
                requestId, requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "[{RequestId}] Error handling request {RequestName} after {ElapsedTime}ms: {ErrorMessage}", 
                requestId, requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            
            throw;
        }
    }
    
    /// <summary>
    /// L?c thông tin nh?y c?m t? request tr??c khi ghi log
    /// </summary>
    private object SanitizeRequest(TRequest request)
    {
        // ?ây là n?i b?n có th? tri?n khai logic ?? l?c các tr??ng nh?y c?m nh? m?t kh?u, token, v.v.
        // Ví d?:
        if (request == null)
            return null;
        
        // ??n gi?n là ch? tr? v? ki?u c?a request n?u b?n không mu?n log n?i dung
        // return typeof(TRequest).Name;
        
        // Ho?c b?n có th? tri?n khai logic l?c ph?c t?p h?n
        // Ví d?: clone request và gán giá tr? "[REDACTED]" cho các tr??ng nh?y c?m
        
        return request;
    }
}