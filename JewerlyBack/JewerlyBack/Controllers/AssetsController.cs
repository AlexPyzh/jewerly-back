using JewerlyBack.Application.Interfaces;
using JewerlyBack.Application.Models;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер для работы с загруженными файлами (изображения, паттерны, текстуры).
/// </summary>
/// <remarks>
/// Архитектура:
/// - Контроллер — thin layer, только валидация входных данных и HTTP
/// - Бизнес-логика — в IAssetService
/// - IO операции — в IS3StorageService
///
/// Безопасность:
/// - Все операции привязаны к userId (проверка владения)
/// - Валидация типов и размеров файлов на уровне сервиса
/// - multipart/form-data для загрузки файлов
/// - Требуется JWT аутентификация для всех endpoints
/// </remarks>
[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IAssetService assetService, ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    /// <summary>
    /// Получить пагинированный список ассетов текущего пользователя
    /// </summary>
    /// <param name="pagination">Параметры пагинации (page, pageSize)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пагинированный список загруженных файлов</returns>
    /// <remarks>
    /// Пример запроса: GET /api/assets?page=1&amp;pageSize=20
    ///
    /// По умолчанию: page=1, pageSize=20
    /// Максимальный pageSize: 100
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UploadedAssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UploadedAssetDto>>> GetUserAssets(
        [FromQuery] PaginationQuery pagination,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var result = await _assetService.GetUserAssetsAsync(userId, pagination, ct);
        return Ok(result);
    }

    /// <summary>
    /// Получить информацию о конкретном ассете
    /// </summary>
    /// <param name="id">ID ассета</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Детальная информация об ассете</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UploadedAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UploadedAssetDto>> GetAsset([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var asset = await _assetService.GetAssetAsync(userId, id, ct);

        if (asset is null)
        {
            _logger.LogWarning("Asset {AssetId} not found or access denied for user {UserId}", id, userId);
            return NotFound(new { message = $"Asset with ID {id} not found or access denied" });
        }

        return Ok(asset);
    }

    /// <summary>
    /// Загрузить новый файл
    /// </summary>
    /// <param name="request">Данные для загрузки файла</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Информация о загруженном файле</returns>
    /// <remarks>
    /// Ограничения:
    /// - Максимальный размер: 10 MB (TODO: настроить для production)
    /// - Разрешённые форматы: PNG, JPG, JPEG, GIF, WebP, SVG
    /// - Разрешённые типы: image, pattern, texture, reference
    ///
    /// Пример curl:
    /// curl -X POST "http://localhost:5000/api/assets/upload" \
    ///   -H "X-User-Id: 00000000-0000-0000-0000-000000000001" \
    ///   -F "file=@/path/to/image.png" \
    ///   -F "fileType=image"
    /// </remarks>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AssetUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB — синхронизировать с AssetService.MaxFileSizeBytes
    public async Task<ActionResult<AssetUploadResponse>> UploadAsset(
        [FromForm] AssetUploadRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        // Базовая валидация на уровне контроллера
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { message = "File is required" });
        }

        if (string.IsNullOrWhiteSpace(request.FileType))
        {
            return BadRequest(new { message = "File type is required" });
        }

        try
        {
            var assetId = await _assetService.UploadAssetAsync(userId, request.File, request.FileType, request.ConfigurationId, ct);

            // Получаем созданный ассет для возврата полной информации
            var asset = await _assetService.GetAssetAsync(userId, assetId, ct);

            var response = new AssetUploadResponse
            {
                Id = assetId,
                Url = asset?.Url ?? string.Empty,
                OriginalFileName = asset?.OriginalFileName,
                FileType = asset?.FileType ?? request.FileType,
                Message = "Asset uploaded successfully"
            };

            return CreatedAtAction(nameof(GetAsset), new { id = assetId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request from user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить ассет
    /// </summary>
    /// <param name="id">ID ассета</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <remarks>
    /// Удаляет файл из S3 и метаданные из БД.
    /// Операция необратима.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsset([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var success = await _assetService.DeleteAssetAsync(userId, id, ct);

        if (!success)
        {
            _logger.LogWarning("Failed to delete asset {AssetId} for user {UserId}", id, userId);
            return NotFound(new { message = $"Asset with ID {id} not found or access denied" });
        }

        return Ok(new { message = "Asset deleted successfully" });
    }

    /// <summary>
    /// Привязать ассет к конфигурации
    /// </summary>
    /// <param name="id">ID ассета</param>
    /// <param name="request">Данные для привязки</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат операции</returns>
    [HttpPost("{id:guid}/attach")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AttachToConfiguration(
        [FromRoute] Guid id,
        [FromBody] AttachAssetRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        if (request.ConfigurationId == Guid.Empty)
        {
            return BadRequest(new { message = "ConfigurationId is required" });
        }

        var success = await _assetService.AttachToConfigurationAsync(userId, id, request.ConfigurationId, ct);

        if (!success)
        {
            return NotFound(new { message = "Asset or configuration not found, or access denied" });
        }

        return Ok(new { message = "Asset attached to configuration successfully" });
    }
}

/// <summary>
/// Ответ при успешной загрузке файла
/// </summary>
public class AssetUploadResponse
{
    /// <summary>
    /// ID созданного ассета
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// URL загруженного файла
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Тип файла
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение о результате операции
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Запрос на привязку ассета к конфигурации
/// </summary>
public class AttachAssetRequest
{
    /// <summary>
    /// ID конфигурации для привязки
    /// </summary>
    public Guid ConfigurationId { get; set; }
}
