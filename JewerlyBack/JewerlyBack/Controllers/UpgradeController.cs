using JewerlyBack.Application.Interfaces;
using JewerlyBack.Dto.Upgrade;
using JewerlyBack.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Controller for the jewelry upgrade flow
/// </summary>
/// <remarks>
/// Handles the complete upgrade flow:
/// 1. Image upload
/// 2. AI analysis
/// 3. Suggestions retrieval
/// 4. Enhanced preview generation
/// </remarks>
[ApiController]
[Route("api/upgrade")]
public class UpgradeController : ControllerBase
{
    private readonly IUpgradeService _upgradeService;
    private readonly ILogger<UpgradeController> _logger;

    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "image/heic" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public UpgradeController(
        IUpgradeService upgradeService,
        ILogger<UpgradeController> logger)
    {
        _upgradeService = upgradeService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a jewelry image for analysis
    /// </summary>
    /// <param name="file">The image file to upload (JPEG, PNG, WebP, or HEIC)</param>
    /// <param name="guestClientId">Guest client ID for anonymous users</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Analysis session ID and image URL</returns>
    /// <remarks>
    /// Uploads the image and initiates AI analysis. The analysis runs in the background.
    /// Poll GET /api/upgrade/analysis/{id} to check analysis status.
    ///
    /// Example request:
    ///     POST /api/upgrade/upload
    ///     Content-Type: multipart/form-data
    ///     [file: image data]
    ///     [guestClientId: optional, required for anonymous users]
    /// </remarks>
    [HttpPost("upload")]
    [AllowAnonymous]
    [RequestSizeLimit(MaxFileSize)]
    [ProducesResponseType(typeof(UpgradeImageUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpgradeImageUploadResponseDto>> UploadImage(
        IFormFile file,
        [FromForm] string? guestClientId,
        CancellationToken ct)
    {
        // Validate file presence
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No image file provided" });
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { message = "File size exceeds 10 MB limit" });
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Invalid file type. Allowed: JPEG, PNG, WebP, HEIC" });
        }

        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _upgradeService.UploadImageAsync(
                stream,
                file.FileName,
                file.ContentType,
                userId,
                guestClientId,
                ct);

            _logger.LogInformation(
                "Image uploaded successfully for {UserType}. Analysis ID: {AnalysisId}",
                userId.HasValue ? $"user {userId.Value}" : $"guest {guestClientId}",
                result.AnalysisId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get analysis results for an uploaded image
    /// </summary>
    /// <param name="id">Analysis session ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Analysis results including detected jewelry type, materials, and stones</returns>
    /// <remarks>
    /// Returns the AI analysis results once processing is complete.
    /// If analysis is still in progress, returns 404.
    ///
    /// Example request:
    ///     GET /api/upgrade/analysis/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpGet("analysis/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UpgradeAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpgradeAnalysisResultDto>> GetAnalysis(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        var result = await _upgradeService.GetAnalysisAsync(id, userId, ct);

        if (result == null)
        {
            return NotFound(new { message = "Analysis not found or access denied" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get upgrade suggestions for an analyzed piece
    /// </summary>
    /// <param name="id">Analysis session ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of suggested improvements grouped by category</returns>
    /// <remarks>
    /// Returns improvement suggestions based on the AI analysis.
    /// All suggestions are optional - the user can keep the original design.
    ///
    /// Categories include:
    /// - Material refinement
    /// - Stone enhancement
    /// - Proportions and balance
    /// - Craftsmanship and detailing
    ///
    /// Example request:
    ///     GET /api/upgrade/suggestions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpGet("suggestions/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UpgradeSuggestionsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpgradeSuggestionsResponseDto>> GetSuggestions(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        var result = await _upgradeService.GetSuggestionsAsync(id, userId, ct);

        if (result == null)
        {
            return NotFound(new { message = "Analysis not found, not yet completed, or access denied" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create an AI preview job with selected enhancements
    /// </summary>
    /// <param name="request">Preview request with selected suggestion IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Preview job with initial status</returns>
    /// <remarks>
    /// Generates an AI preview of the jewelry with selected enhancements.
    /// Poll GET /api/upgrade/preview/{id} to check generation status.
    ///
    /// Example request:
    ///     POST /api/upgrade/preview
    ///     {
    ///       "analysisId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "selectedSuggestionIds": ["guid1", "guid2"],
    ///       "keepOriginal": false,
    ///       "guestClientId": "optional-for-anonymous"
    ///     }
    ///
    /// To keep the original design without enhancements, set keepOriginal to true.
    /// </remarks>
    [HttpPost("preview")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UpgradePreviewJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UpgradePreviewJobDto>> CreatePreview(
        [FromBody] UpgradePreviewRequestDto request,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        try
        {
            var job = await _upgradeService.CreatePreviewJobAsync(request, userId, ct);

            _logger.LogInformation(
                "Preview job {JobId} created for analysis {AnalysisId}",
                job.Id, request.AnalysisId);

            return AcceptedAtAction(
                nameof(GetPreview),
                new { id = job.Id },
                job);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid preview request");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized preview request");
            return Forbid();
        }
    }

    /// <summary>
    /// Get preview job status and result
    /// </summary>
    /// <param name="id">Preview job ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Preview job with current status and result URLs</returns>
    /// <remarks>
    /// Returns the current status of preview generation.
    ///
    /// Possible statuses:
    /// - Pending (0): Waiting in queue
    /// - Processing (1): AI is generating
    /// - Completed (2): Ready - check enhancedImageUrl
    /// - Failed (3): Error - check errorMessage
    ///
    /// Example request:
    ///     GET /api/upgrade/preview/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpGet("preview/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UpgradePreviewJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpgradePreviewJobDto>> GetPreview(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? User.GetCurrentUserId()
            : null;

        var result = await _upgradeService.GetPreviewJobAsync(id, userId, ct);

        if (result == null)
        {
            return NotFound(new { message = "Preview job not found or access denied" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user's recent upgrade analyses
    /// </summary>
    /// <param name="take">Number of analyses to return (default 5, max 20)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of recent analyses</returns>
    /// <remarks>
    /// Returns the user's recent completed analyses, sorted by date.
    /// Requires authentication.
    ///
    /// Example request:
    ///     GET /api/upgrade/recent?take=5
    /// </remarks>
    [HttpGet("recent")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<UpgradeAnalysisResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UpgradeAnalysisResultDto>>> GetRecentAnalyses(
        [FromQuery] int take = 5,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 20);

        var userId = User.GetCurrentUserId();
        var result = await _upgradeService.GetRecentAnalysesAsync(userId, take, ct);

        return Ok(result);
    }
}
