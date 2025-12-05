using System.Text.Json;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с AI превью ювелирных изделий
/// </summary>
public class AiPreviewService : IAiPreviewService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AiPreviewService> _logger;

    public AiPreviewService(
        AppDbContext context,
        ILogger<AiPreviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AiPreviewJobDto> CreateJobAsync(
        CreateAiPreviewRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        // Валидация типа превью
        if (!Enum.IsDefined(typeof(AiPreviewType), request.Type))
        {
            _logger.LogWarning(
                "Invalid preview type {Type} provided for configuration {ConfigurationId}",
                request.Type, request.ConfigurationId);
            throw new ArgumentException($"Invalid preview type: {request.Type}", nameof(request.Type));
        }

        _logger.LogInformation(
            "Creating AI preview job for configuration {ConfigurationId}, user {UserId}, type {Type}",
            request.ConfigurationId, userId, request.Type);

        // Проверяем, что конфигурация существует и принадлежит пользователю
        var configuration = await _context.JewelryConfigurations
            .AsNoTracking()
            .Where(c => c.Id == request.ConfigurationId)
            .Select(c => new { c.Id, c.UserId })
            .FirstOrDefaultAsync(ct);

        if (configuration == null)
        {
            _logger.LogWarning(
                "Configuration {ConfigurationId} not found",
                request.ConfigurationId);
            throw new ArgumentException($"Configuration {request.ConfigurationId} not found");
        }

        if (configuration.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access configuration {ConfigurationId} owned by {OwnerId}",
                userId, request.ConfigurationId, configuration.UserId);
            throw new UnauthorizedAccessException("Configuration does not belong to current user");
        }

        var now = DateTimeOffset.UtcNow;

        var job = new AiPreviewJob
        {
            Id = Guid.NewGuid(),
            ConfigurationId = request.ConfigurationId,
            Type = request.Type,
            Status = AiPreviewStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _context.AiPreviewJobs.Add(job);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "AI preview job {JobId} created successfully for configuration {ConfigurationId}",
            job.Id, request.ConfigurationId);

        // TODO (Step 7.1): После создания Job запустить фоновую обработку
        // либо через BackgroundService, либо через Hangfire/Quartz
        // Пример: await _backgroundJobQueue.EnqueueAsync(job.Id);

        return MapToDto(job);
    }

    public async Task<AiPreviewJobDto?> GetJobAsync(
        Guid jobId,
        Guid userId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Getting AI preview job {JobId} for user {UserId}",
            jobId, userId);

        var job = await _context.AiPreviewJobs
            .AsNoTracking()
            .Where(j => j.Id == jobId)
            .Include(j => j.Configuration)
            .FirstOrDefaultAsync(ct);

        if (job == null)
        {
            _logger.LogWarning("AI preview job {JobId} not found", jobId);
            return null;
        }

        // Проверяем, что job относится к конфигурации текущего пользователя
        if (job.Configuration.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access job {JobId} owned by {OwnerId}",
                userId, jobId, job.Configuration.UserId);
            return null;
        }

        return MapToDto(job);
    }

    /// <summary>
    /// Маппинг сущности AiPreviewJob в DTO
    /// </summary>
    private AiPreviewJobDto MapToDto(AiPreviewJob job)
    {
        IReadOnlyList<string>? frameUrls = null;

        // Парсим FramesJson, если есть
        if (!string.IsNullOrWhiteSpace(job.FramesJson))
        {
            try
            {
                frameUrls = JsonSerializer.Deserialize<List<string>>(job.FramesJson)?.AsReadOnly();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to parse FramesJson for job {JobId}", job.Id);
            }
        }

        return new AiPreviewJobDto
        {
            Id = job.Id,
            ConfigurationId = job.ConfigurationId,
            Type = job.Type,
            Status = job.Status,
            SingleImageUrl = job.SingleImageUrl,
            FrameUrls = frameUrls,
            ErrorMessage = job.ErrorMessage,
            CreatedAtUtc = job.CreatedAtUtc,
            UpdatedAtUtc = job.UpdatedAtUtc
        };
    }

    // TODO (Step 7.1): Метод для реальной обработки AI
    // public async Task ProcessJobAsync(AiPreviewJob job, CancellationToken ct = default)
    // {
    //     // 1. Обновить статус на Processing
    //     // 2. Сформировать промпт на основе конфигурации
    //     // 3. Вызвать AI провайдера (Stable Diffusion / DALL-E / Midjourney)
    //     // 4. Загрузить результат в S3
    //     // 5. Обновить job с результатом (Completed) или ошибкой (Failed)
    // }
}
