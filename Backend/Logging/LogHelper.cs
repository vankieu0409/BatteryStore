using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BatteryShop.Logging;

/// <summary>
/// Cung c?p c�c ph??ng th?c ti?n �ch ?? ghi log
/// </summary>
public static class LogHelper
{
    /// <summary>
    /// Ghi log th?i gian th?c hi?n c?a m?t ?o?n m�
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">T�n c?a ho?t ??ng</param>
    /// <param name="action">?o?n m� c?n th?c hi?n v� ?o th?i gian</param>
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
    /// Ghi log th?i gian th?c hi?n c?a m?t ?o?n m� b?t ??ng b?
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">T�n c?a ho?t ??ng</param>
    /// <param name="func">?o?n m� b?t ??ng b? c?n th?c hi?n v� ?o th?i gian</param>
    /// <typeparam name="T">Ki?u c?a logger</typeparam>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a h�m</typeparam>
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
    /// Th�m th�ng tin v�o LogContext cho m?t ?o?n m�
    /// </summary>
    /// <param name="propertyName">T�n thu?c t�nh</param>
    /// <param name="value">Gi� tr? thu?c t�nh</param>
    /// <param name="action">?o?n m� c?n th?c hi?n trong context</param>
    public static void WithContext(string propertyName, object value, Action action)
    {
        using (LogContext.PushProperty(propertyName, value))
        {
            action();
        }
    }
    
    /// <summary>
    /// Th�m th�ng tin v�o LogContext cho m?t ?o?n m� b?t ??ng b?
    /// </summary>
    /// <param name="propertyName">T�n thu?c t�nh</param>
    /// <param name="value">Gi� tr? thu?c t�nh</param>
    /// <param name="func">?o?n m� b?t ??ng b? c?n th?c hi?n trong context</param>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a h�m</typeparam>
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
    /// <param name="userId">ID c?a user (n?u c�)</param>
    /// <param name="func">?o?n m� b?t ??ng b? c?n th?c hi?n trong context</param>
    /// <typeparam name="TResult">Ki?u k?t qu? tr? v? c?a h�m</typeparam>
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