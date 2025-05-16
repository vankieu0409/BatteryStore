using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BatteryShop.Logging;

/// <summary>
/// Cung c?p các ph??ng th?c ti?n ích ?? ghi log
/// </summary>
public static class LogHelper
{
    /// <summary>
    /// Ghi log th?i gian th?c hi?n c?a m?t ?o?n mã
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Tên c?a ho?t ??ng</param>
    /// <param name="action">?o?n mã c?n th?c hi?n và ?o th?i gian</param>
    /// <typeparam name="T">Ki?u c?a logger</typeparam>
    public static void LogPerformance<T>(ILogger<T> logger, string operationName, Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            action();
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation("{OperationName} completed in {ElapsedMilliseconds}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
        }
    }
    
    /// <summary>
    /// Ghi log th?i gian th?c hi?n c?a m?t ?o?n mã b?t ??ng b?
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Tên c?a ho?t ??ng</param>
    /// <param name="func">?o?n mã b?t ??ng b? c?n th?c hi?n và ?o th?i gian</param>
    /// <typeparam name="T">Ki?u c?a logger</typeparam>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a hàm</typeparam>
    /// <returns>K?t qu? tr? v? t? func</returns>
    public static async Task<TResult> LogPerformanceAsync<T, TResult>(
        ILogger<T> logger, string operationName, Func<Task<TResult>> func)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await func();
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation("{OperationName} completed in {ElapsedMilliseconds}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
        }
    }
    
    /// <summary>
    /// Thêm thông tin vào LogContext cho m?t ?o?n mã
    /// </summary>
    /// <param name="propertyName">Tên thu?c tính</param>
    /// <param name="value">Giá tr? thu?c tính</param>
    /// <param name="action">?o?n mã c?n th?c hi?n trong context</param>
    public static void WithContext(string propertyName, object value, Action action)
    {
        using (LogContext.PushProperty(propertyName, value))
        {
            action();
        }
    }
    
    /// <summary>
    /// Thêm thông tin vào LogContext cho m?t ?o?n mã b?t ??ng b?
    /// </summary>
    /// <param name="propertyName">Tên thu?c tính</param>
    /// <param name="value">Giá tr? thu?c tính</param>
    /// <param name="func">?o?n mã b?t ??ng b? c?n th?c hi?n trong context</param>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a hàm</typeparam>
    /// <returns>K?t qu? tr? v? t? func</returns>
    public static async Task<TResult> WithContextAsync<TResult>(
        string propertyName, object value, Func<Task<TResult>> func)
    {
        using (LogContext.PushProperty(propertyName, value))
        {
            return await func();
        }
    }
    
    /// <summary>
    /// T?o LogContext cho m?t request c? th?
    /// </summary>
    /// <param name="requestId">ID c?a request</param>
    /// <param name="userId">ID c?a user (n?u có)</param>
    /// <param name="func">?o?n mã b?t ??ng b? c?n th?c hi?n trong context</param>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a hàm</typeparam>
    /// <returns>K?t qu? tr? v? t? func</returns>
    public static async Task<TResult> WithRequestContext<TResult>(
        string requestId, string? userId, Func<Task<TResult>> func)
    {
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        {
            return await func();
        }
    }
}