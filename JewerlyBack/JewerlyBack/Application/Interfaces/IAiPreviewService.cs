using JewerlyBack.Dto;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с AI превью ювелирных изделий
/// </summary>
public interface IAiPreviewService
{
    /// <summary>
    /// Создать новое задание на генерацию AI превью
    /// </summary>
    /// <param name="request">Данные запроса</param>
    /// <param name="userId">ID текущего пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO созданного задания</returns>
    /// <exception cref="UnauthorizedAccessException">Если конфигурация не принадлежит пользователю</exception>
    /// <exception cref="ArgumentException">Если конфигурация не найдена</exception>
    Task<AiPreviewJobDto> CreateJobAsync(
        CreateAiPreviewRequest request,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Получить статус задания AI превью
    /// </summary>
    /// <param name="jobId">ID задания</param>
    /// <param name="userId">ID текущего пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO задания или null если не найдено</returns>
    Task<AiPreviewJobDto?> GetJobAsync(
        Guid jobId,
        Guid userId,
        CancellationToken ct = default);

    // TODO (Step 7.1): Реализовать метод ProcessJobAsync для реальной обработки AI
    // Task ProcessJobAsync(AiPreviewJob job, CancellationToken ct = default);
}
