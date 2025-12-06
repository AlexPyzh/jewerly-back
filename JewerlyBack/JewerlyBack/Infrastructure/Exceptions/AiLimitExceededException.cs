namespace JewerlyBack.Infrastructure.Exceptions;

/// <summary>
/// Исключение, которое выбрасывается, когда гость превышает лимит бесплатных AI генераций
/// </summary>
public class AiLimitExceededException : Exception
{
    /// <summary>
    /// Количество доступных бесплатных генераций
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// ID гостя, превысившего лимит
    /// </summary>
    public string GuestClientId { get; }

    public AiLimitExceededException(string guestClientId, int limit)
        : base($"Free AI preview limit ({limit}) reached for guest {guestClientId}. Please sign up to continue.")
    {
        GuestClientId = guestClientId;
        Limit = limit;
    }

    public AiLimitExceededException(string guestClientId, int limit, string message)
        : base(message)
    {
        GuestClientId = guestClientId;
        Limit = limit;
    }

    public AiLimitExceededException(string guestClientId, int limit, string message, Exception innerException)
        : base(message, innerException)
    {
        GuestClientId = guestClientId;
        Limit = limit;
    }
}
