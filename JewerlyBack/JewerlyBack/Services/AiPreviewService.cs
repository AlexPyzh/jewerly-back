using System.Text.Json;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Exceptions;
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
        Guid? userId,
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

        var now = DateTimeOffset.UtcNow;

        // Различаем авторизованного пользователя и гостя
        if (userId.HasValue)
        {
            // Авторизованный пользователь
            _logger.LogInformation(
                "Creating AI preview job for configuration {ConfigurationId}, user {UserId}, type {Type}",
                request.ConfigurationId, userId.Value, request.Type);

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

            if (configuration.UserId != userId.Value)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access configuration {ConfigurationId} owned by {OwnerId}",
                    userId.Value, request.ConfigurationId, configuration.UserId);
                throw new UnauthorizedAccessException("Configuration does not belong to current user");
            }

            var job = new AiPreviewJob
            {
                Id = Guid.NewGuid(),
                ConfigurationId = request.ConfigurationId,
                UserId = userId.Value,
                GuestClientId = null,
                Type = request.Type,
                Status = AiPreviewStatus.Pending,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.AiPreviewJobs.Add(job);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "AI preview job {JobId} created successfully for user {UserId}",
                job.Id, userId.Value);

            return MapToDto(job);
        }
        else
        {
            // Гость (анонимный пользователь)
            var guestClientId = request.GuestClientId;

            if (string.IsNullOrWhiteSpace(guestClientId))
            {
                _logger.LogWarning(
                    "GuestClientId is required for anonymous AI preview requests");
                throw new ArgumentException("GuestClientId is required for anonymous users", nameof(request.GuestClientId));
            }

            _logger.LogInformation(
                "Creating AI preview job for guest {GuestClientId}, configuration {ConfigurationId}, type {Type}",
                guestClientId, request.ConfigurationId, request.Type);

            // Проверяем лимит для гостя
            const int maxFreeGuestJobs = 5;

            var completedCount = await _context.AiPreviewJobs
                .Where(j => j.GuestClientId == guestClientId
                            && j.UserId == null
                            && j.Status == AiPreviewStatus.Completed)
                .CountAsync(ct);

            if (completedCount >= maxFreeGuestJobs)
            {
                _logger.LogWarning(
                    "Guest {GuestClientId} exceeded free AI preview limit ({Limit})",
                    guestClientId, maxFreeGuestJobs);
                throw new AiLimitExceededException(guestClientId, maxFreeGuestJobs);
            }

            // Проверяем, что конфигурация существует
            // Для гостей разрешаем использовать любую валидную конфигурацию (MVP)
            var configurationExists = await _context.JewelryConfigurations
                .AnyAsync(c => c.Id == request.ConfigurationId, ct);

            if (!configurationExists)
            {
                _logger.LogWarning(
                    "Configuration {ConfigurationId} not found",
                    request.ConfigurationId);
                throw new ArgumentException($"Configuration {request.ConfigurationId} not found");
            }

            var job = new AiPreviewJob
            {
                Id = Guid.NewGuid(),
                ConfigurationId = request.ConfigurationId,
                UserId = null,
                GuestClientId = guestClientId,
                Type = request.Type,
                Status = AiPreviewStatus.Pending,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.AiPreviewJobs.Add(job);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "AI preview job {JobId} created successfully for guest {GuestClientId} ({CompletedCount}/{Limit})",
                job.Id, guestClientId, completedCount + 1, maxFreeGuestJobs);

            return MapToDto(job);
        }

        // TODO (Step 7.1): После создания Job запустить фоновую обработку
        // либо через BackgroundService, либо через Hangfire/Quartz
        // Пример: await _backgroundJobQueue.EnqueueAsync(job.Id);
    }

    public async Task<AiPreviewJobDto?> GetJobAsync(
        Guid jobId,
        Guid? userId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Getting AI preview job {JobId} for user {UserId}",
            jobId, userId?.ToString() ?? "guest");

        var job = await _context.AiPreviewJobs
            .AsNoTracking()
            .Where(j => j.Id == jobId)
            .FirstOrDefaultAsync(ct);

        if (job == null)
        {
            _logger.LogWarning("AI preview job {JobId} not found", jobId);
            return null;
        }

        // Проверка прав доступа
        if (job.UserId.HasValue)
        {
            // Job принадлежит авторизованному пользователю
            if (!userId.HasValue)
            {
                // Гость пытается получить job авторизованного пользователя
                _logger.LogWarning(
                    "Guest attempted to access job {JobId} owned by user {UserId}",
                    jobId, job.UserId.Value);
                return null;
            }

            if (job.UserId.Value != userId.Value)
            {
                // Другой пользователь пытается получить чужой job
                _logger.LogWarning(
                    "User {UserId} attempted to access job {JobId} owned by user {OwnerId}",
                    userId.Value, jobId, job.UserId.Value);
                return null;
            }
        }
        else
        {
            // Job принадлежит гостю (job.UserId == null)
            // Для MVP: разрешаем получить job по знанию GUID
            // (риск минимальный, т.к. GUID сложно подобрать)
            _logger.LogInformation(
                "Returning guest job {JobId} (guestClientId: {GuestClientId})",
                jobId, job.GuestClientId ?? "unknown");
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
