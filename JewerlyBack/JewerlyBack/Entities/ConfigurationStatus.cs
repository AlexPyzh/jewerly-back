namespace JewerlyBack.Models;

/// <summary>
/// Status of a jewelry configuration
/// </summary>
public enum ConfigurationStatus
{
    /// <summary>
    /// Configuration is in draft state
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Configuration is ready to be ordered
    /// </summary>
    ReadyToOrder = 1,

    /// <summary>
    /// Configuration is part of an order
    /// </summary>
    InOrder = 2
}
