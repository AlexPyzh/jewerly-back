using JewerlyBack.Application.Interfaces;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер для работы с AI превью ювелирных изделий
/// </summary>
/// <remarks>
/// Endpoints доступны как для авторизованных пользователей, так и для гостей.
/// Гости имеют ограничение в 5 бесплатных AI генераций.
/// </remarks>
[ApiController]
[Route("api/ai/preview")]
public class AiPreviewController : ControllerBase
{
    private readonly IAiPreviewService _aiPreviewService;
    private readonly ILogger<AiPreviewController> _logger;

    public AiPreviewController(
        IAiPreviewService aiPreviewService,
        ILogger<AiPreviewController> logger)
    {
        _aiPreviewService = aiPreviewService;
        _logger = logger;
    }

    /// <summary>
    /// Создать новое задание на генерацию AI превью
    /// </summary>
    /// <param name="request">Данные запроса (ConfigurationId, Type, GuestClientId для гостей)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO созданного задания со статусом Pending</returns>
    /// <remarks>
    /// Пример запроса для авторизованного пользователя:
    ///
    ///     POST /api/ai/preview
    ///     Authorization: Bearer {token}
    ///     {
    ///       "configurationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "type": 0  // 0 = SingleImage, 1 = Preview360
    ///     }
    ///
    /// Пример запроса для гостя (без Authorization заголовка):
    ///
    ///     POST /api/ai/preview
    ///     {
    ///       "configurationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "type": 0,
    ///       "guestClientId": "550e8400-e29b-41d4-a716-446655440000"
    ///     }
    ///
    /// Гости ограничены 5 бесплатными генерациями. При превышении лимита вернётся HTTP 429.
    ///
    /// После создания задание будет иметь статус Pending.
    /// Используйте GET /api/ai/preview/{id} для проверки статуса обработки.
    ///
    /// TODO (Step 7.1): В будущем здесь будет запускаться фоновая обработка через AI провайдера.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AiPreviewJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AiPreviewJobDto>> CreatePreviewJob(
        [FromBody] CreateAiPreviewRequest request,
        CancellationToken ct)
    {
        // Получаем userId: если пользователь авторизован - берём из Claims, иначе null
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        try
        {
            var job = await _aiPreviewService.CreateJobAsync(request, userId, ct);

            _logger.LogInformation(
                "AI preview job {JobId} created for {UserType}, configuration {ConfigurationId}",
                job.Id,
                userId.HasValue ? $"user {userId.Value}" : $"guest {request.GuestClientId}",
                request.ConfigurationId);

            // 202 Accepted - асинхронная операция принята в обработку
            return AcceptedAtAction(
                nameof(GetPreviewJob),
                new { id = job.Id },
                job);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "User {UserId} unauthorized to access configuration {ConfigurationId}",
                userId, request.ConfigurationId);
            return Forbid();
        }
    }

    /// <summary>
    /// Получить статус задания AI превью по ID
    /// </summary>
    /// <param name="id">ID задания</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO задания с текущим статусом и результатами (если готово)</returns>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     GET /api/ai/preview/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// Доступно как для авторизованных пользователей, так и для гостей.
    ///
    /// Возможные статусы:
    /// - Pending (0) - ожидает обработки
    /// - Processing (1) - генерируется AI
    /// - Completed (2) - готово, смотрите singleImageUrl или frameUrls
    /// - Failed (3) - ошибка, смотрите errorMessage
    ///
    /// Фронтенд должен периодически опрашивать этот endpoint (polling),
    /// пока статус не станет Completed или Failed.
    /// </remarks>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AiPreviewJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AiPreviewJobDto>> GetPreviewJob(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        // Получаем userId: если пользователь авторизован - берём из Claims, иначе null
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        var job = await _aiPreviewService.GetJobAsync(id, userId, ct);

        if (job == null)
        {
            _logger.LogWarning(
                "AI preview job {JobId} not found or access denied for {UserType}",
                id, userId.HasValue ? $"user {userId.Value}" : "guest");
            return NotFound(new { message = $"AI preview job {id} not found or access denied" });
        }

        return Ok(job);
    }
}
