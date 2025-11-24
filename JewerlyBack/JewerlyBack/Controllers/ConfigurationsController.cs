using JewerlyBack.Application.Interfaces;
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
    /// Получить список конфигураций текущего пользователя
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JewelryConfigurationListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JewelryConfigurationListItemDto>>> GetUserConfigurations(
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var configurations = await _configurationService.GetUserConfigurationsAsync(userId, ct);
        return Ok(configurations);
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
    [HttpPost]
    [ProducesResponseType(typeof(CreateConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateConfigurationResponse>> CreateConfiguration(
        [FromBody] JewelryConfigurationCreateRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

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
    /// Обновить существующую конфигурацию
    /// </summary>
    /// <param name="id">ID конфигурации</param>
    /// <param name="request">Данные для обновления</param>
    /// <param name="ct">Токен отмены</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateConfiguration(
        [FromRoute] Guid id,
        [FromBody] JewelryConfigurationUpdateRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        try
        {
            var success = await _configurationService.UpdateConfigurationAsync(userId, id, request, ct);

            if (!success)
            {
                _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                    id, userId);
                return NotFound(new { message = $"Configuration with ID {id} not found or access denied" });
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
