namespace JewerlyBack.Entities;

/// <summary>
/// Entity for tracking audit events
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }

    /// <summary>
    /// User who performed the action (null for anonymous users)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Type of entity being audited (e.g., "JewelryConfiguration", "Order")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// ID of the entity being audited
    /// </summary>
    public required string EntityId { get; set; }

    /// <summary>
    /// Action performed (e.g., "Created", "Updated", "Deleted")
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// JSON representation of changes made
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// IP address of the client (optional)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client (optional)
    /// </summary>
    public string? UserAgent { get; set; }
}
