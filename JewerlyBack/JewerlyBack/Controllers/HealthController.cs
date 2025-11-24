using JewerlyBack.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Controllers;

/// <summary>
/// Health check endpoints для Kubernetes/Docker orchestration.
/// </summary>
/// <remarks>
/// Liveness probe — проверяет, что приложение запущено и отвечает.
/// Readiness probe — проверяет, что приложение готово принимать трафик (все зависимости доступны).
///
/// Kubernetes конфигурация:
/// ```yaml
/// livenessProbe:
///   httpGet:
///     path: /api/health/live
///     port: 80
///   initialDelaySeconds: 5
///   periodSeconds: 10
///
/// readinessProbe:
///   httpGet:
///     path: /api/health/ready
///     port: 80
///   initialDelaySeconds: 10
///   periodSeconds: 5
/// ```
/// </remarks>
[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe — приложение живо и отвечает
    /// </summary>
    /// <returns>200 OK если приложение запущено</returns>
    /// <remarks>
    /// Используется для определения, нужно ли перезапустить контейнер.
    /// Не проверяет внешние зависимости — только то, что процесс работает.
    /// </remarks>
    [HttpGet("live")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Live()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Readiness probe — приложение готово принимать трафик
    /// </summary>
    /// <returns>200 OK если все зависимости доступны, 503 если есть проблемы</returns>
    /// <remarks>
    /// Проверяет:
    /// - Подключение к PostgreSQL
    ///
    /// Если readiness падает — Kubernetes перестаёт направлять трафик на этот pod,
    /// но НЕ перезапускает его (в отличие от liveness).
    /// </remarks>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ReadinessResponse>> Ready(CancellationToken ct)
    {
        var response = new ReadinessResponse
        {
            Timestamp = DateTimeOffset.UtcNow,
            Checks = new List<HealthCheckResult>()
        };

        var isHealthy = true;

        // Проверка PostgreSQL
        var dbCheck = await CheckDatabaseAsync(ct);
        response.Checks.Add(dbCheck);
        if (dbCheck.Status != "Healthy")
        {
            isHealthy = false;
        }

        response.Status = isHealthy ? "Healthy" : "Unhealthy";

        if (!isHealthy)
        {
            _logger.LogWarning("Readiness check failed: {Checks}",
                string.Join(", ", response.Checks.Where(c => c.Status != "Healthy").Select(c => $"{c.Name}: {c.Description}")));

            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Детальная информация о состоянии приложения (для мониторинга/отладки)
    /// </summary>
    /// <returns>Расширенная информация о здоровье</returns>
    /// <remarks>
    /// Включает версию приложения, uptime и детали по каждой зависимости.
    /// Не использовать для probes — слишком тяжёлый.
    /// </remarks>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DetailedHealthResponse>> Detailed(CancellationToken ct)
    {
        var response = new DetailedHealthResponse
        {
            Timestamp = DateTimeOffset.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Checks = new List<HealthCheckResult>()
        };

        // Проверка PostgreSQL
        response.Checks.Add(await CheckDatabaseAsync(ct));

        response.Status = response.Checks.All(c => c.Status == "Healthy") ? "Healthy" : "Unhealthy";

        return Ok(response);
    }

    /// <summary>
    /// Проверка подключения к базе данных
    /// </summary>
    private async Task<HealthCheckResult> CheckDatabaseAsync(CancellationToken ct)
    {
        var result = new HealthCheckResult { Name = "PostgreSQL" };
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Простой запрос для проверки подключения
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", ct);

            sw.Stop();
            result.Status = "Healthy";
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Description = "Connection successful";
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Status = "Unhealthy";
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Description = $"Connection failed: {ex.Message}";

            _logger.LogError(ex, "Database health check failed");
        }

        return result;
    }
}

/// <summary>
/// Базовый ответ health check
/// </summary>
public class HealthResponse
{
    /// <summary>
    /// Статус: Healthy, Unhealthy, Degraded
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Время проверки
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Ответ readiness probe с детализацией проверок
/// </summary>
public class ReadinessResponse : HealthResponse
{
    /// <summary>
    /// Результаты отдельных проверок
    /// </summary>
    public List<HealthCheckResult> Checks { get; set; } = new();
}

/// <summary>
/// Детальный ответ для мониторинга
/// </summary>
public class DetailedHealthResponse : ReadinessResponse
{
    /// <summary>
    /// Версия приложения
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Окружение (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = string.Empty;
}

/// <summary>
/// Результат отдельной проверки здоровья
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Название проверяемого компонента
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Статус: Healthy, Unhealthy, Degraded
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Описание результата
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Время ответа в миллисекундах
    /// </summary>
    public long? ResponseTimeMs { get; set; }
}
