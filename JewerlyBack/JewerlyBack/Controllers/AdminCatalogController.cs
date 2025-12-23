using JewerlyBack.Application.Models;
using JewerlyBack.Data;
using JewerlyBack.Dto.Admin;
using JewerlyBack.Infrastructure.Storage;
using JewerlyBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Controllers;

/// <summary>
/// Admin controller for managing catalog data (categories, base models, materials, stones)
/// </summary>
[ApiController]
[Route("api/admin/catalog")]
[Authorize(Policy = "AdminOnly")]
public class AdminCatalogController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IS3StorageService _storageService;
    private readonly ILogger<AdminCatalogController> _logger;

    public AdminCatalogController(
        AppDbContext context,
        IS3StorageService storageService,
        ILogger<AdminCatalogController> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }

    #region Categories

    /// <summary>
    /// Get all categories with pagination
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(PagedResult<AdminCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminCategoryDto>>> GetCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.JewelryCategories.AsQueryable();

        var totalCount = await query.CountAsync(ct);

        var categories = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                AiCategoryDescription = c.AiCategoryDescription,
                IsActive = c.IsActive,
                ImageUrl = null
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminCategoryDto>
        {
            Items = categories,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get a single category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryDto>> GetCategory(int id, CancellationToken ct = default)
    {
        var category = await _context.JewelryCategories
            .Where(c => c.Id == id)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                AiCategoryDescription = c.AiCategoryDescription,
                IsActive = c.IsActive,
                ImageUrl = null
            })
            .FirstOrDefaultAsync(ct);

        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminCategoryDto>> CreateCategory(
        [FromBody] AdminCategoryCreateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingCategory = await _context.JewelryCategories
            .FirstOrDefaultAsync(c => c.Code == request.Code, ct);

        if (existingCategory != null)
        {
            return Conflict(new { message = $"Category with code '{request.Code}' already exists" });
        }

        var category = new JewelryCategory
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            AiCategoryDescription = request.AiCategoryDescription,
            IsActive = request.IsActive
        };

        _context.JewelryCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Category created: {Code} (ID: {Id})", category.Code, category.Id);

        var dto = new AdminCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            AiCategoryDescription = category.AiCategoryDescription,
            IsActive = category.IsActive,
            ImageUrl = null
        };

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, dto);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("categories/{id}")]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminCategoryDto>> UpdateCategory(
        int id,
        [FromBody] AdminCategoryUpdateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _context.JewelryCategories.FindAsync(new object[] { id }, ct);

        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        var codeConflict = await _context.JewelryCategories
            .AnyAsync(c => c.Code == request.Code && c.Id != id, ct);

        if (codeConflict)
        {
            return Conflict(new { message = $"Category with code '{request.Code}' already exists" });
        }

        category.Code = request.Code;
        category.Name = request.Name;
        category.Description = request.Description;
        category.AiCategoryDescription = request.AiCategoryDescription;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Category updated: {Code} (ID: {Id})", category.Code, category.Id);

        var dto = new AdminCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            AiCategoryDescription = category.AiCategoryDescription,
            IsActive = category.IsActive,
            ImageUrl = null
        };

        return Ok(dto);
    }

    /// <summary>
    /// Delete a category (soft delete by setting IsActive to false)
    /// </summary>
    [HttpDelete("categories/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct = default)
    {
        var category = await _context.JewelryCategories
            .Include(c => c.BaseModels)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        var hasActiveModels = category.BaseModels.Any(m => m.IsActive);

        if (hasActiveModels)
        {
            return Conflict(new
            {
                message = "Cannot delete category with active base models. Deactivate all base models first."
            });
        }

        category.IsActive = false;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Category soft deleted: {Code} (ID: {Id})", category.Code, category.Id);

        return NoContent();
    }

    /// <summary>
    /// Upload or replace category image
    /// </summary>
    [HttpPost("categories/{id}/image")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UploadCategoryImage(
        int id,
        [FromForm] IFormFile file,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var category = await _context.JewelryCategories.FindAsync(new object[] { id }, ct);

        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds 10MB limit" });
        }

        try
        {
            var fileExtension = Path.GetExtension(file.FileName);
            var fileKey = $"catalog/categories/{category.Code}/{Guid.NewGuid()}{fileExtension}";

            using var stream = file.OpenReadStream();
            var imageUrl = await _storageService.UploadAsync(stream, fileKey, file.ContentType, ct);

            _logger.LogInformation("Category image uploaded: {Code} -> {Url}", category.Code, imageUrl);

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload category image for {Code}", category.Code);
            return StatusCode(500, new { message = "Failed to upload image" });
        }
    }

    #endregion

    #region Base Models

    /// <summary>
    /// Get all base models with pagination and optional category filter
    /// </summary>
    [HttpGet("base-models")]
    [ProducesResponseType(typeof(PagedResult<AdminBaseModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminBaseModelDto>>> GetBaseModels(
        [FromQuery] int? categoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.JewelryBaseModels.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var models = await query
            .Include(m => m.Category)
            .OrderBy(m => m.Category.Name)
            .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new AdminBaseModelDto
            {
                Id = m.Id,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                Code = m.Code,
                Name = m.Name,
                Description = m.Description,
                AiDescription = m.AiDescription,
                PreviewImageUrl = m.PreviewImageUrl,
                BasePrice = m.BasePrice,
                IsActive = m.IsActive,
                MetadataJson = m.MetadataJson
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminBaseModelDto>
        {
            Items = models,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get a single base model by ID
    /// </summary>
    [HttpGet("base-models/{id}")]
    [ProducesResponseType(typeof(AdminBaseModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminBaseModelDto>> GetBaseModel(Guid id, CancellationToken ct = default)
    {
        var model = await _context.JewelryBaseModels
            .Include(m => m.Category)
            .Where(m => m.Id == id)
            .Select(m => new AdminBaseModelDto
            {
                Id = m.Id,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                Code = m.Code,
                Name = m.Name,
                Description = m.Description,
                AiDescription = m.AiDescription,
                PreviewImageUrl = m.PreviewImageUrl,
                BasePrice = m.BasePrice,
                IsActive = m.IsActive,
                MetadataJson = m.MetadataJson
            })
            .FirstOrDefaultAsync(ct);

        if (model == null)
        {
            return NotFound(new { message = $"Base model with ID {id} not found" });
        }

        return Ok(model);
    }

    /// <summary>
    /// Create a new base model
    /// </summary>
    [HttpPost("base-models")]
    [ProducesResponseType(typeof(AdminBaseModelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminBaseModelDto>> CreateBaseModel(
        [FromBody] AdminBaseModelCreateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var categoryExists = await _context.JewelryCategories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            return BadRequest(new { message = $"Category with ID {request.CategoryId} not found" });
        }

        var existingModel = await _context.JewelryBaseModels
            .FirstOrDefaultAsync(m => m.Code == request.Code && m.CategoryId == request.CategoryId, ct);

        if (existingModel != null)
        {
            return Conflict(new { message = $"Base model with code '{request.Code}' already exists in this category" });
        }

        var model = new JewelryBaseModel
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            AiDescription = request.AiDescription,
            PreviewImageUrl = request.PreviewImageUrl,
            BasePrice = request.BasePrice,
            IsActive = request.IsActive,
            MetadataJson = request.MetadataJson
        };

        _context.JewelryBaseModels.Add(model);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Base model created: {Code} (ID: {Id})", model.Code, model.Id);

        var category = await _context.JewelryCategories.FindAsync(new object[] { request.CategoryId }, ct);

        var dto = new AdminBaseModelDto
        {
            Id = model.Id,
            CategoryId = model.CategoryId,
            CategoryName = category?.Name ?? "",
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            AiDescription = model.AiDescription,
            PreviewImageUrl = model.PreviewImageUrl,
            BasePrice = model.BasePrice,
            IsActive = model.IsActive,
            MetadataJson = model.MetadataJson
        };

        return CreatedAtAction(nameof(GetBaseModel), new { id = model.Id }, dto);
    }

    /// <summary>
    /// Update an existing base model
    /// </summary>
    [HttpPut("base-models/{id}")]
    [ProducesResponseType(typeof(AdminBaseModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminBaseModelDto>> UpdateBaseModel(
        Guid id,
        [FromBody] AdminBaseModelUpdateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var model = await _context.JewelryBaseModels.FindAsync(new object[] { id }, ct);

        if (model == null)
        {
            return NotFound(new { message = $"Base model with ID {id} not found" });
        }

        var categoryExists = await _context.JewelryCategories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            return BadRequest(new { message = $"Category with ID {request.CategoryId} not found" });
        }

        var codeConflict = await _context.JewelryBaseModels
            .AnyAsync(m => m.Code == request.Code && m.CategoryId == request.CategoryId && m.Id != id, ct);

        if (codeConflict)
        {
            return Conflict(new { message = $"Base model with code '{request.Code}' already exists in this category" });
        }

        model.CategoryId = request.CategoryId;
        model.Code = request.Code;
        model.Name = request.Name;
        model.Description = request.Description;
        model.AiDescription = request.AiDescription;
        model.PreviewImageUrl = request.PreviewImageUrl;
        model.BasePrice = request.BasePrice;
        model.IsActive = request.IsActive;
        model.MetadataJson = request.MetadataJson;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Base model updated: {Code} (ID: {Id})", model.Code, model.Id);

        var category = await _context.JewelryCategories.FindAsync(new object[] { request.CategoryId }, ct);

        var dto = new AdminBaseModelDto
        {
            Id = model.Id,
            CategoryId = model.CategoryId,
            CategoryName = category?.Name ?? "",
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            AiDescription = model.AiDescription,
            PreviewImageUrl = model.PreviewImageUrl,
            BasePrice = model.BasePrice,
            IsActive = model.IsActive,
            MetadataJson = model.MetadataJson
        };

        return Ok(dto);
    }

    /// <summary>
    /// Delete a base model (soft delete by setting IsActive to false)
    /// </summary>
    [HttpDelete("base-models/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBaseModel(Guid id, CancellationToken ct = default)
    {
        var model = await _context.JewelryBaseModels.FindAsync(new object[] { id }, ct);

        if (model == null)
        {
            return NotFound(new { message = $"Base model with ID {id} not found" });
        }

        model.IsActive = false;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Base model soft deleted: {Code} (ID: {Id})", model.Code, model.Id);

        return NoContent();
    }

    /// <summary>
    /// Upload or replace base model image
    /// </summary>
    [HttpPost("base-models/{id}/image")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UploadBaseModelImage(
        Guid id,
        [FromForm] IFormFile file,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var model = await _context.JewelryBaseModels.FindAsync(new object[] { id }, ct);

        if (model == null)
        {
            return NotFound(new { message = $"Base model with ID {id} not found" });
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds 10MB limit" });
        }

        try
        {
            var fileExtension = Path.GetExtension(file.FileName);
            var fileKey = $"catalog/base-models/{model.Code}/{Guid.NewGuid()}{fileExtension}";

            using var stream = file.OpenReadStream();
            var imageUrl = await _storageService.UploadAsync(stream, fileKey, file.ContentType, ct);

            model.PreviewImageUrl = imageUrl;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Base model image uploaded: {Code} -> {Url}", model.Code, imageUrl);

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload base model image for {Code}", model.Code);
            return StatusCode(500, new { message = "Failed to upload image" });
        }
    }

    #endregion

    #region Materials

    /// <summary>
    /// Get all materials with pagination
    /// </summary>
    [HttpGet("materials")]
    [ProducesResponseType(typeof(PagedResult<AdminMaterialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminMaterialDto>>> GetMaterials(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Materials.AsQueryable();

        var totalCount = await query.CountAsync(ct);

        var materials = await query
            .OrderBy(m => m.MetalType)
            .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new AdminMaterialDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                MetalType = m.MetalType,
                Karat = m.Karat,
                ColorHex = m.ColorHex,
                PriceFactor = m.PriceFactor,
                IsActive = m.IsActive
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminMaterialDto>
        {
            Items = materials,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get a single material by ID
    /// </summary>
    [HttpGet("materials/{id}")]
    [ProducesResponseType(typeof(AdminMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminMaterialDto>> GetMaterial(int id, CancellationToken ct = default)
    {
        var material = await _context.Materials
            .Where(m => m.Id == id)
            .Select(m => new AdminMaterialDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                MetalType = m.MetalType,
                Karat = m.Karat,
                ColorHex = m.ColorHex,
                PriceFactor = m.PriceFactor,
                IsActive = m.IsActive
            })
            .FirstOrDefaultAsync(ct);

        if (material == null)
        {
            return NotFound(new { message = $"Material with ID {id} not found" });
        }

        return Ok(material);
    }

    /// <summary>
    /// Create a new material
    /// </summary>
    [HttpPost("materials")]
    [ProducesResponseType(typeof(AdminMaterialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminMaterialDto>> CreateMaterial(
        [FromBody] AdminMaterialCreateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingMaterial = await _context.Materials
            .FirstOrDefaultAsync(m => m.Code == request.Code, ct);

        if (existingMaterial != null)
        {
            return Conflict(new { message = $"Material with code '{request.Code}' already exists" });
        }

        var material = new Material
        {
            Code = request.Code,
            Name = request.Name,
            MetalType = request.MetalType,
            Karat = request.Karat,
            ColorHex = request.ColorHex,
            PriceFactor = request.PriceFactor,
            IsActive = request.IsActive
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Material created: {Code} (ID: {Id})", material.Code, material.Id);

        var dto = new AdminMaterialDto
        {
            Id = material.Id,
            Code = material.Code,
            Name = material.Name,
            MetalType = material.MetalType,
            Karat = material.Karat,
            ColorHex = material.ColorHex,
            PriceFactor = material.PriceFactor,
            IsActive = material.IsActive
        };

        return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, dto);
    }

    /// <summary>
    /// Update an existing material
    /// </summary>
    [HttpPut("materials/{id}")]
    [ProducesResponseType(typeof(AdminMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminMaterialDto>> UpdateMaterial(
        int id,
        [FromBody] AdminMaterialUpdateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var material = await _context.Materials.FindAsync(new object[] { id }, ct);

        if (material == null)
        {
            return NotFound(new { message = $"Material with ID {id} not found" });
        }

        var codeConflict = await _context.Materials
            .AnyAsync(m => m.Code == request.Code && m.Id != id, ct);

        if (codeConflict)
        {
            return Conflict(new { message = $"Material with code '{request.Code}' already exists" });
        }

        material.Code = request.Code;
        material.Name = request.Name;
        material.MetalType = request.MetalType;
        material.Karat = request.Karat;
        material.ColorHex = request.ColorHex;
        material.PriceFactor = request.PriceFactor;
        material.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Material updated: {Code} (ID: {Id})", material.Code, material.Id);

        var dto = new AdminMaterialDto
        {
            Id = material.Id,
            Code = material.Code,
            Name = material.Name,
            MetalType = material.MetalType,
            Karat = material.Karat,
            ColorHex = material.ColorHex,
            PriceFactor = material.PriceFactor,
            IsActive = material.IsActive
        };

        return Ok(dto);
    }

    /// <summary>
    /// Delete a material (soft delete by setting IsActive to false)
    /// </summary>
    [HttpDelete("materials/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMaterial(int id, CancellationToken ct = default)
    {
        var material = await _context.Materials.FindAsync(new object[] { id }, ct);

        if (material == null)
        {
            return NotFound(new { message = $"Material with ID {id} not found" });
        }

        material.IsActive = false;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Material soft deleted: {Code} (ID: {Id})", material.Code, material.Id);

        return NoContent();
    }

    #endregion

    #region Stone Types

    /// <summary>
    /// Get all stone types with pagination
    /// </summary>
    [HttpGet("stone-types")]
    [ProducesResponseType(typeof(PagedResult<AdminStoneTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminStoneTypeDto>>> GetStoneTypes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.StoneTypes.AsQueryable();

        var totalCount = await query.CountAsync(ct);

        var stoneTypes = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AdminStoneTypeDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Color = s.Color,
                DefaultPricePerCarat = s.DefaultPricePerCarat,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminStoneTypeDto>
        {
            Items = stoneTypes,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get a single stone type by ID
    /// </summary>
    [HttpGet("stone-types/{id}")]
    [ProducesResponseType(typeof(AdminStoneTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminStoneTypeDto>> GetStoneType(int id, CancellationToken ct = default)
    {
        var stoneType = await _context.StoneTypes
            .Where(s => s.Id == id)
            .Select(s => new AdminStoneTypeDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Color = s.Color,
                DefaultPricePerCarat = s.DefaultPricePerCarat,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(ct);

        if (stoneType == null)
        {
            return NotFound(new { message = $"Stone type with ID {id} not found" });
        }

        return Ok(stoneType);
    }

    /// <summary>
    /// Create a new stone type
    /// </summary>
    [HttpPost("stone-types")]
    [ProducesResponseType(typeof(AdminStoneTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminStoneTypeDto>> CreateStoneType(
        [FromBody] AdminStoneTypeCreateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingStoneType = await _context.StoneTypes
            .FirstOrDefaultAsync(s => s.Code == request.Code, ct);

        if (existingStoneType != null)
        {
            return Conflict(new { message = $"Stone type with code '{request.Code}' already exists" });
        }

        var stoneType = new StoneType
        {
            Code = request.Code,
            Name = request.Name,
            Color = request.Color,
            DefaultPricePerCarat = request.DefaultPricePerCarat,
            IsActive = request.IsActive
        };

        _context.StoneTypes.Add(stoneType);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Stone type created: {Code} (ID: {Id})", stoneType.Code, stoneType.Id);

        var dto = new AdminStoneTypeDto
        {
            Id = stoneType.Id,
            Code = stoneType.Code,
            Name = stoneType.Name,
            Color = stoneType.Color,
            DefaultPricePerCarat = stoneType.DefaultPricePerCarat,
            IsActive = stoneType.IsActive
        };

        return CreatedAtAction(nameof(GetStoneType), new { id = stoneType.Id }, dto);
    }

    /// <summary>
    /// Update an existing stone type
    /// </summary>
    [HttpPut("stone-types/{id}")]
    [ProducesResponseType(typeof(AdminStoneTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminStoneTypeDto>> UpdateStoneType(
        int id,
        [FromBody] AdminStoneTypeUpdateRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var stoneType = await _context.StoneTypes.FindAsync(new object[] { id }, ct);

        if (stoneType == null)
        {
            return NotFound(new { message = $"Stone type with ID {id} not found" });
        }

        var codeConflict = await _context.StoneTypes
            .AnyAsync(s => s.Code == request.Code && s.Id != id, ct);

        if (codeConflict)
        {
            return Conflict(new { message = $"Stone type with code '{request.Code}' already exists" });
        }

        stoneType.Code = request.Code;
        stoneType.Name = request.Name;
        stoneType.Color = request.Color;
        stoneType.DefaultPricePerCarat = request.DefaultPricePerCarat;
        stoneType.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Stone type updated: {Code} (ID: {Id})", stoneType.Code, stoneType.Id);

        var dto = new AdminStoneTypeDto
        {
            Id = stoneType.Id,
            Code = stoneType.Code,
            Name = stoneType.Name,
            Color = stoneType.Color,
            DefaultPricePerCarat = stoneType.DefaultPricePerCarat,
            IsActive = stoneType.IsActive
        };

        return Ok(dto);
    }

    /// <summary>
    /// Delete a stone type (soft delete by setting IsActive to false)
    /// </summary>
    [HttpDelete("stone-types/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStoneType(int id, CancellationToken ct = default)
    {
        var stoneType = await _context.StoneTypes.FindAsync(new object[] { id }, ct);

        if (stoneType == null)
        {
            return NotFound(new { message = $"Stone type with ID {id} not found" });
        }

        stoneType.IsActive = false;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Stone type soft deleted: {Code} (ID: {Id})", stoneType.Code, stoneType.Id);

        return NoContent();
    }

    #endregion
}
