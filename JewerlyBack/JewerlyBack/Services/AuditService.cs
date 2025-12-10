using System.Text.Json;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JewerlyBack.Services;

/// <summary>
/// Implementation of audit logging service
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
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
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Changes = details != null ? JsonSerializer.Serialize(details, JsonOptions) : null,
                Timestamp = DateTimeOffset.UtcNow,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext?.Request.Headers.UserAgent.FirstOrDefault()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(ct);

            _logger.LogDebug(
                "Audit logged: {Action} {EntityType}/{EntityId} by user {UserId}",
                action, entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            // Audit logging should not break the main flow
            _logger.LogWarning(ex,
                "Failed to log audit event: {Action} {EntityType}/{EntityId}",
                action, entityType, entityId);
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
