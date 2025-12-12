using JewerlyBack.Application.Interfaces;
using JewerlyBack.Application.Models;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер для работы с конфигурациями ювелирных изделий
/// </summary>
/// <remarks>
/// Все endpoints требуют аутентификации - работают только с данными текущего пользователя.
/// </remarks>
[ApiController]
[Route("api/configurations")]
[Authorize]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationsController> _logger;

    public ConfigurationsController(
        IConfigurationService configurationService,
        ILogger<ConfigurationsController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Получить последние конфигурации пользователя
    /// </summary>
    /// <param name="take">Количество элементов (по умолчанию 5, максимум 20)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список последних конфигураций</returns>
    /// <remarks>
    /// Пример запроса: GET /api/configurations/recent?take=5
    ///
    /// Возвращает последние конфигурации, отсортированные по дате обновления (UpdatedAt DESC).
    /// </remarks>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IReadOnlyList<JewelryConfigurationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JewelryConfigurationSummaryDto>>> GetRecent(
        [FromQuery] int take = 5,
        CancellationToken ct = default)
    {
        var userId = User.GetCurrentUserId();
        var items = await _configurationService.GetRecentForUserAsync(userId, take, ct);
        return Ok(items);
    }

    /// <summary>
    /// Получить пагинированный список конфигураций текущего пользователя
    /// </summary>
    /// <param name="pagination">Параметры пагинации (page, pageSize)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пагинированный список конфигураций</returns>
    /// <remarks>
    /// Пример запроса: GET /api/configurations?page=1&amp;pageSize=20
    ///
    /// По умолчанию: page=1, pageSize=20
    /// Максимальный pageSize: 100
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JewelryConfigurationListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<JewelryConfigurationListItemDto>>> GetUserConfigurations(
        [FromQuery] PaginationQuery pagination,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var result = await _configurationService.GetUserConfigurationsAsync(userId, pagination, ct);
        return Ok(result);
    }

    /// <summary>
    /// Получить детальную информацию о конфигурации
    /// </summary>
    /// <param name="id">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JewelryConfigurationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JewelryConfigurationDetailDto>> GetConfigurationById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var configuration = await _configurationService.GetConfigurationByIdAsync(userId, id, ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                id, userId);
            return NotFound(new { message = $"Configuration with ID {id} not found or access denied" });
        }

        return Ok(configuration);
    }

    /// <summary>
    /// Создать новую конфигурацию
    /// </summary>
    /// <param name="request">Данные для создания конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <remarks>
    /// Позволяет создавать конфигурации как для авторизованных, так и для анонимных пользователей (для AI preview).
    /// Для анонимных пользователей UserId будет null.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateConfigurationResponse>> CreateConfiguration(
        [FromBody] JewelryConfigurationCreateRequest request,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true ? User.GetCurrentUserId() : null;

        _logger.LogInformation(
            "📥 CreateConfiguration: userId={UserId}, baseModelId={BaseModelId}, materialId={MaterialId}, configJson={ConfigJson}",
            userId, request.BaseModelId, request.MaterialId, request.ConfigJson);

        try
        {
            var configurationId = await _configurationService.CreateConfigurationAsync(userId, request, ct);

            var response = new CreateConfigurationResponse
            {
                Id = configurationId,
                Message = "Configuration created successfully"
            };

            return CreatedAtAction(
                nameof(GetConfigurationById),
                new { id = configurationId },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating configuration");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Сохранить конфигурацию (создать новую или обновить существующую)
    /// </summary>
    /// <param name="request">Данные конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <remarks>
    /// Позволяет сохранять конфигурации как для авторизованных, так и для анонимных пользователей (для AI preview).
    /// Если ConfigurationId не указан или конфигурация не найдена, создаёт новую.
    /// Для анонимных пользователей UserId будет null.
    /// Всегда возвращает полную конфигурацию с актуальным ID.
    /// </remarks>
    [HttpPost("save")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JewelryConfigurationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JewelryConfigurationDetailDto>> SaveConfiguration(
        [FromBody] JewelryConfigurationSaveRequest request,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true ? User.GetCurrentUserId() : null;

        _logger.LogInformation(
            "📥 SaveConfiguration: userId={UserId}, configId={ConfigId}, baseModelId={BaseModelId}, materialId={MaterialId}",
            userId, request.ConfigurationId, request.BaseModelId, request.MaterialId);

        try
        {
            var updateRequest = new JewelryConfigurationUpdateRequest
            {
                MaterialId = request.MaterialId,
                Name = request.Name,
                ConfigJson = request.ConfigJson,
                Status = request.Status,
                EngravingText = request.EngravingText,
                Stones = request.Stones,
                Engravings = request.Engravings
            };

            var configuration = await _configurationService.SaveOrUpdateConfigurationAsync(
                userId,
                request.ConfigurationId,
                request.BaseModelId,
                request.MaterialId,
                updateRequest,
                ct);

            _logger.LogInformation(
                "✅ Configuration saved successfully: id={ConfigurationId}",
                configuration.Id);

            return Ok(configuration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for saving configuration");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить существующую конфигурацию
    /// </summary>
    /// <param name="id">ID конфигурации</param>
    /// <param name="request">Данные для обновления</param>
    /// <param name="ct">Токен отмены</param>
    /// <remarks>
    /// Позволяет обновлять конфигурации как для авторизованных, так и для анонимных пользователей (для AI preview).
    /// Для анонимных пользователей UserId будет null.
    /// DEPRECATED: используйте POST /api/configurations/save для более надёжного flow.
    /// </remarks>
    [HttpPut("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateConfiguration(
        [FromRoute] Guid id,
        [FromBody] JewelryConfigurationUpdateRequest request,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true ? User.GetCurrentUserId() : null;

        try
        {
            var success = await _configurationService.UpdateConfigurationAsync(userId, id, request, ct);

            if (!success)
            {
                _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                    id, userId);
                return NotFound(new
                {
                    status = 404,
                    error = "NotFound",
                    message = "Configuration not found or outdated. It may have been deleted or modified by another session."
                });
            }

            return Ok(new { message = "Configuration updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating configuration {ConfigurationId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить конфигурацию
    /// </summary>
    /// <param name="id">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConfiguration(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var success = await _configurationService.DeleteConfigurationAsync(userId, id, ct);

        if (!success)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                id, userId);
            return NotFound(new { message = $"Configuration with ID {id} not found or access denied" });
        }

        return Ok(new { message = "Configuration deleted successfully" });
    }
}

/// <summary>
/// Ответ при успешном создании конфигурации
/// </summary>
public class CreateConfigurationResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
