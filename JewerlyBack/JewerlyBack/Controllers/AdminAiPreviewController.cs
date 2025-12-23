using JewerlyBack.Application.Models;
using JewerlyBack.Data;
using JewerlyBack.Dto.Admin;
using JewerlyBack.Entities;
using JewerlyBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Controllers;

/// <summary>
/// Admin controller for viewing AI preview history
/// </summary>
[ApiController]
[Route("api/admin/ai-previews")]
[Authorize(Policy = "AdminOnly")]
public class AdminAiPreviewController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminAiPreviewController> _logger;

    public AdminAiPreviewController(AppDbContext context, ILogger<AdminAiPreviewController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all AI preview jobs with pagination and filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="type">Filter by type</param>
    /// <param name="fromDate">Filter by creation date (from)</param>
    /// <param name="toDate">Filter by creation date (to)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminAiPreviewJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminAiPreviewJobDto>>> GetAiPreviewJobs(
        [FromQuery] AiPreviewStatus? status = null,
        [FromQuery] AiPreviewType? type = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.AiPreviewJobs
            .Include(j => j.Configuration)
            .ThenInclude(c => c.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(j => j.Type == type.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(j => j.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(j => j.CreatedAtUtc <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var jobs = await query
            .OrderByDescending(j => j.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new AdminAiPreviewJobDto
            {
                Id = j.Id,
                ConfigurationId = j.ConfigurationId,
                UserId = j.UserId,
                UserEmail = j.Configuration.User != null ? j.Configuration.User.Email : null,
                GuestClientId = j.GuestClientId,
                Type = j.Type,
                TypeName = j.Type.ToString(),
                Status = j.Status,
                StatusName = j.Status.ToString(),
                Prompt = j.Prompt,
                AiConfigJson = j.AiConfigJson,
                ErrorMessage = j.ErrorMessage,
                SingleImageUrl = j.SingleImageUrl,
                FramesJson = j.FramesJson,
                CreatedAtUtc = j.CreatedAtUtc,
                UpdatedAtUtc = j.UpdatedAtUtc
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminAiPreviewJobDto>
        {
            Items = jobs,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get a single AI preview job with full details
    /// </summary>
    /// <param name="id">Job ID</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminAiPreviewJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAiPreviewJobDto>> GetAiPreviewJob(Guid id, CancellationToken ct = default)
    {
        var job = await _context.AiPreviewJobs
            .Include(j => j.Configuration)
            .ThenInclude(c => c.User)
            .Where(j => j.Id == id)
            .Select(j => new AdminAiPreviewJobDto
            {
                Id = j.Id,
                ConfigurationId = j.ConfigurationId,
                UserId = j.UserId,
                UserEmail = j.Configuration.User != null ? j.Configuration.User.Email : null,
                GuestClientId = j.GuestClientId,
                Type = j.Type,
                TypeName = j.Type.ToString(),
                Status = j.Status,
                StatusName = j.Status.ToString(),
                Prompt = j.Prompt,
                AiConfigJson = j.AiConfigJson,
                ErrorMessage = j.ErrorMessage,
                SingleImageUrl = j.SingleImageUrl,
                FramesJson = j.FramesJson,
                CreatedAtUtc = j.CreatedAtUtc,
                UpdatedAtUtc = j.UpdatedAtUtc
            })
            .FirstOrDefaultAsync(ct);

        if (job == null)
        {
            return NotFound(new { message = $"AI preview job with ID {id} not found" });
        }

        return Ok(job);
    }

    /// <summary>
    /// Get statistics about AI preview jobs
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStatistics(CancellationToken ct = default)
    {
        var totalJobs = await _context.AiPreviewJobs.CountAsync(ct);
        var pendingJobs = await _context.AiPreviewJobs.CountAsync(j => j.Status == AiPreviewStatus.Pending, ct);
        var processingJobs = await _context.AiPreviewJobs.CountAsync(j => j.Status == AiPreviewStatus.Processing, ct);
        var completedJobs = await _context.AiPreviewJobs.CountAsync(j => j.Status == AiPreviewStatus.Completed, ct);
        var failedJobs = await _context.AiPreviewJobs.CountAsync(j => j.Status == AiPreviewStatus.Failed, ct);

        var totalUsers = await _context.AiPreviewJobs
            .Where(j => j.UserId != null)
            .Select(j => j.UserId)
            .Distinct()
            .CountAsync(ct);

        var totalGuests = await _context.AiPreviewJobs
            .Where(j => j.GuestClientId != null)
            .Select(j => j.GuestClientId)
            .Distinct()
            .CountAsync(ct);

        var singleImageJobs = await _context.AiPreviewJobs.CountAsync(j => j.Type == AiPreviewType.SingleImage, ct);
        var preview360Jobs = await _context.AiPreviewJobs.CountAsync(j => j.Type == AiPreviewType.Preview360, ct);

        return Ok(new
        {
            totalJobs,
            byStatus = new
            {
                pending = pendingJobs,
                processing = processingJobs,
                completed = completedJobs,
                failed = failedJobs
            },
            byType = new
            {
                singleImage = singleImageJobs,
                preview360 = preview360Jobs
            },
            users = new
            {
                totalUsers,
                totalGuests
            }
        });
    }
}
