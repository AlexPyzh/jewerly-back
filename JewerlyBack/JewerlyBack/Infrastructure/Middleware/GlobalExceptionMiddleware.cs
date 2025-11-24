using System.Net;
using System.Text.Json;

namespace JewerlyBack.Infrastructure.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений.
/// Перехватывает все необработанные исключения и возвращает стандартизированный JSON-ответ.
/// </summary>
/// <remarks>
/// Безопасность:
/// - В Production НЕ раскрывает stack trace и детали исключений
/// - В Development показывает полную информацию для отладки
/// - Все ошибки логируются с correlation ID
/// </remarks>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        // Логируем ошибку
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        // Определяем статус код и сообщение в зависимости от типа исключения
        var (statusCode, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            OperationCanceledException => (HttpStatusCode.BadRequest, "Request was cancelled"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Status = (int)statusCode,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        };

        // В Development показываем детали
        if (_environment.IsDevelopment())
        {
            response.Details = exception.ToString();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Стандартизированный ответ об ошибке
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус код
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID корреляции для отслеживания в логах
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Время возникновения ошибки
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Детали ошибки (только в Development)
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Extension методы для регистрации middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
