using System.Diagnostics;

namespace JewerlyBack.Infrastructure.Middleware;

/// <summary>
/// Middleware для логирования HTTP запросов.
/// Записывает информацию о каждом запросе: метод, путь, статус, время выполнения.
/// </summary>
/// <remarks>
/// Полезно для:
/// - Мониторинга производительности
/// - Отладки проблем
/// - Аудита
///
/// Не логирует тела запросов/ответов (может содержать sensitive data).
/// </remarks>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;

        // Пропускаем health endpoints для уменьшения шума в логах
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);

            sw.Stop();

            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                : context.Response.StatusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
        catch
        {
            sw.Stop();

            _logger.LogError(
                "HTTP {Method} {Path} failed after {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds,
                correlationId);

            throw; // Пробрасываем дальше для GlobalExceptionMiddleware
        }
    }
}

/// <summary>
/// Extension методы для регистрации middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
