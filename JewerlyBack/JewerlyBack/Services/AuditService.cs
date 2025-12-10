using System.Text.Json;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JewerlyBack.Services;

/// <summary>
/// Implementation of audit logging service.
/// Uses IServiceScopeFactory to create fresh DbContext instances for each audit operation,
/// ensuring audit logging works correctly even when called from fire-and-forget contexts
/// or after the original HTTP request scope has ended.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogCreateAsync(
        Guid? userId,
        string entityType,
        string entityId,
        object? details = null,
        CancellationToken ct = default)
    {
        await LogActionAsync(userId, entityType, entityId, "Created", details, ct);
    }

    public async Task LogUpdateAsync(
        Guid? userId,
        string entityType,
        string entityId,
        object? changes = null,
        CancellationToken ct = default)
    {
        await LogActionAsync(userId, entityType, entityId, "Updated", changes, ct);
    }

    public async Task LogDeleteAsync(
        Guid? userId,
        string entityType,
        string entityId,
        CancellationToken ct = default)
    {
        await LogActionAsync(userId, entityType, entityId, "Deleted", null, ct);
    }

    public async Task LogActionAsync(
        Guid? userId,
        string entityType,
        string entityId,
        string action,
        object? details = null,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Audit logging starting: {Action} {EntityType}/{EntityId} by user {UserId}",
            action, entityType, entityId, userId);

        try
        {
            // Capture HTTP context information before creating scope
            // (HttpContext may not be available in the new scope)
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers.UserAgent.FirstOrDefault();

            // Create a new scope to get a fresh DbContext instance
            // This ensures audit logging works even if called after the original request scope ends
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogDebug("Audit: DbContext obtained from new scope for {EntityType}/{EntityId}", entityType, entityId);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Changes = details != null ? JsonSerializer.Serialize(details, JsonOptions) : null,
                Timestamp = DateTimeOffset.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync(ct);

            _logger.LogDebug(
                "Audit record saved successfully: {Action} {EntityType}/{EntityId} by user {UserId}",
                action, entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            // Audit logging should not break the main flow
            _logger.LogWarning(ex,
                "Failed to log audit event: {Action} {EntityType}/{EntityId} for user {UserId}. Exception: {ExceptionType} - {ExceptionMessage}",
                action, entityType, entityId, userId, ex.GetType().Name, ex.Message);
        }
    }

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        // Check for forwarded IP (behind reverse proxy)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP from the comma-separated list
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
