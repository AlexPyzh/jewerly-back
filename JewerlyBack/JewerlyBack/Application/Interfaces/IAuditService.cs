namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Service for logging audit events
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs a create action for an entity
    /// </summary>
    Task LogCreateAsync(
        Guid? userId,
        string entityType,
        string entityId,
        object? details = null,
        CancellationToken ct = default);

    /// <summary>
    /// Logs an update action for an entity
    /// </summary>
    Task LogUpdateAsync(
        Guid? userId,
        string entityType,
        string entityId,
        object? changes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Logs a delete action for an entity
    /// </summary>
    Task LogDeleteAsync(
        Guid? userId,
        string entityType,
        string entityId,
        CancellationToken ct = default);

    /// <summary>
    /// Logs a custom action for an entity
    /// </summary>
    Task LogActionAsync(
        Guid? userId,
        string entityType,
        string entityId,
        string action,
        object? details = null,
        CancellationToken ct = default);
}
