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
/// Все endpoints требуют аутентификации.
/// Пользователь может создавать и просматривать превью только для своих конфигураций.
/// </remarks>
[ApiController]
[Route("api/ai/preview")]
[Authorize]
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
    /// <param name="request">Данные запроса (ConfigurationId, Type)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO созданного задания со статусом Pending</returns>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /api/ai/preview
    ///     {
    ///       "configurationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "type": 0  // 0 = SingleImage, 1 = Preview360
    ///     }
    ///
    /// После создания задание будет иметь статус Pending.
    /// Используйте GET /api/ai/preview/{id} для проверки статуса обработки.
    ///
    /// TODO (Step 7.1): В будущем здесь будет запускаться фоновая обработка через AI провайдера.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(AiPreviewJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AiPreviewJobDto>> CreatePreviewJob(
        [FromBody] CreateAiPreviewRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        try
        {
            var job = await _aiPreviewService.CreateJobAsync(request, userId, ct);

            _logger.LogInformation(
                "AI preview job {JobId} created for user {UserId}, configuration {ConfigurationId}",
                job.Id, userId, request.ConfigurationId);

            // 202 Accepted - асинхронная операция принята в обработку
            return AcceptedAtAction(
                nameof(GetPreviewJob),
                new { id = job.Id },
                job);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration {ConfigurationId}", request.ConfigurationId);
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
    [ProducesResponseType(typeof(AiPreviewJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AiPreviewJobDto>> GetPreviewJob(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        var job = await _aiPreviewService.GetJobAsync(id, userId, ct);

        if (job == null)
        {
            _logger.LogWarning(
                "AI preview job {JobId} not found or access denied for user {UserId}",
                id, userId);
            return NotFound(new { message = $"AI preview job {id} not found or access denied" });
        }

        return Ok(job);
    }
}
