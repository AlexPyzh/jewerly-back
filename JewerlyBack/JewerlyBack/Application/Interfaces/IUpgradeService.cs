using JewerlyBack.Dto.Upgrade;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Service for handling jewelry upgrade flow
/// </summary>
public interface IUpgradeService
{
    /// <summary>
    /// Upload and begin analysis of a jewelry image
    /// </summary>
    /// <param name="imageStream">The uploaded image stream</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="contentType">MIME type of the image</param>
    /// <param name="userId">User ID (null for guests)</param>
    /// <param name="guestClientId">Guest client ID for anonymous users</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Upload response with analysis ID</returns>
    Task<UpgradeImageUploadResponseDto> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        Guid? userId,
        string? guestClientId,
        CancellationToken ct = default);

    /// <summary>
    /// Get the status and results of an analysis
    /// </summary>
    /// <param name="analysisId">Analysis session ID</param>
    /// <param name="userId">User ID (null for guests)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Analysis result or null if not found/unauthorized</returns>
    Task<UpgradeAnalysisResultDto?> GetAnalysisAsync(
        Guid analysisId,
        Guid? userId,
        CancellationToken ct = default);

    /// <summary>
    /// Get upgrade suggestions for an analyzed piece
    /// </summary>
    /// <param name="analysisId">Analysis session ID</param>
    /// <param name="userId">User ID (null for guests)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Suggestions response or null if not found/unauthorized</returns>
    Task<UpgradeSuggestionsResponseDto?> GetSuggestionsAsync(
        Guid analysisId,
        Guid? userId,
        CancellationToken ct = default);

    /// <summary>
    /// Generate an AI preview with selected enhancements
    /// </summary>
    /// <param name="request">Preview request with selected suggestions</param>
    /// <param name="userId">User ID (null for guests)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Preview job DTO</returns>
    Task<UpgradePreviewJobDto> CreatePreviewJobAsync(
        UpgradePreviewRequestDto request,
        Guid? userId,
        CancellationToken ct = default);

    /// <summary>
    /// Get the status of a preview job
    /// </summary>
    /// <param name="jobId">Preview job ID</param>
    /// <param name="userId">User ID (null for guests)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Preview job DTO or null if not found/unauthorized</returns>
    Task<UpgradePreviewJobDto?> GetPreviewJobAsync(
        Guid jobId,
        Guid? userId,
        CancellationToken ct = default);

    /// <summary>
    /// Get user's recent analyses
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="take">Number of items to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of recent analyses</returns>
    Task<IReadOnlyList<UpgradeAnalysisResultDto>> GetRecentAnalysesAsync(
        Guid userId,
        int take = 5,
        CancellationToken ct = default);
}
