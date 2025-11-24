using JewerlyBack.Application.Interfaces;
using JewerlyBack.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер для работы с каталогом изделий
/// </summary>
/// <remarks>
/// Все endpoints публичные - не требуют аутентификации.
/// Используются для отображения каталога товаров всем посетителям.
/// </remarks>
[ApiController]
[Route("api/catalog")]
[AllowAnonymous]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех активных категорий
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<JewelryCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JewelryCategoryDto>>> GetCategories(CancellationToken ct)
    {
        var categories = await _catalogService.GetCategoriesAsync(ct);
        return Ok(categories);
    }

    /// <summary>
    /// Получить список всех активных материалов
    /// </summary>
    [HttpGet("materials")]
    [ProducesResponseType(typeof(IReadOnlyList<MaterialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> GetMaterials(CancellationToken ct)
    {
        var materials = await _catalogService.GetMaterialsAsync(ct);
        return Ok(materials);
    }

    /// <summary>
    /// Получить список всех активных типов камней
    /// </summary>
    [HttpGet("stone-types")]
    [ProducesResponseType(typeof(IReadOnlyList<StoneTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StoneTypeDto>>> GetStoneTypes(CancellationToken ct)
    {
        var stoneTypes = await _catalogService.GetStoneTypesAsync(ct);
        return Ok(stoneTypes);
    }

    /// <summary>
    /// Получить список активных базовых моделей по категории
    /// </summary>
    /// <param name="categoryId">ID категории</param>
    /// <param name="ct">Токен отмены</param>
    [HttpGet("base-models")]
    [ProducesResponseType(typeof(IReadOnlyList<JewelryBaseModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JewelryBaseModelDto>>> GetBaseModelsByCategory(
        [FromQuery] int categoryId,
        CancellationToken ct)
    {
        var baseModels = await _catalogService.GetBaseModelsByCategoryAsync(categoryId, ct);
        return Ok(baseModels);
    }

    /// <summary>
    /// Получить детальную информацию о базовой модели по ID
    /// </summary>
    /// <param name="id">ID базовой модели</param>
    /// <param name="ct">Токен отмены</param>
    [HttpGet("base-models/{id}")]
    [ProducesResponseType(typeof(JewelryBaseModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JewelryBaseModelDto>> GetBaseModelById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var baseModel = await _catalogService.GetBaseModelByIdAsync(id, ct);

        if (baseModel == null)
        {
            _logger.LogWarning("Base model with ID {BaseModelId} not found", id);
            return NotFound(new { message = $"Base model with ID {id} not found" });
        }

        return Ok(baseModel);
    }
}
