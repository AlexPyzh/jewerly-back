namespace JewerlyBack.Infrastructure.Ai.Configuration;

/// <summary>
/// Настройки для интеграции с Leonardo AI API.
///
/// ВАЖНО: ApiKey НЕ должен храниться в appsettings.json!
/// ApiKey автоматически загружается из переменной окружения LEONARDO_API_KEY.
///
/// Установка ключа:
/// - Переменная окружения: LEONARDO_API_KEY=...
/// - Docker/Heroku/Render/GitHub Actions: установите LEONARDO_API_KEY в environment variables
///
/// Валидация ApiKey происходит при старте приложения (ValidateOnStart).
/// Если LEONARDO_API_KEY не установлен, приложение не запустится.
/// </summary>
public sealed class LeonardoAiOptions
{
    /// <summary>
    /// Имя секции в appsettings.json
    /// </summary>
    public const string SectionName = "Ai:Leonardo";

    /// <summary>
    /// API-ключ для доступа к Leonardo AI API.
    /// Загружается из переменной окружения LEONARDO_API_KEY.
    /// Значение из appsettings.json игнорируется и перезаписывается при старте приложения.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// ID модели для генерации изображений.
    /// Для PhotoReal V2 требуется одна из совместимых моделей:
    /// - Leonardo Kino XL: aa77f04e-3eec-4034-9c07-d0f619684628 (cinematic, dramatic)
    /// - Leonardo Vision XL: 5c232a9e-9061-4777-980a-ddc8e65647c6 (photorealistic, portraits)
    /// - Leonardo Diffusion XL: 1e60896f-3c26-4296-8ecc-53e2afecc132 (versatile)
    /// По умолчанию используется Leonardo Kino XL для качественных product renders.
    /// </summary>
    public string ModelId { get; init; } = "aa77f04e-3eec-4034-9c07-d0f619684628";

    /// <summary>
    /// Базовый URL для Leonardo AI API.
    /// </summary>
    public string BaseUrl { get; init; } = "https://cloud.leonardo.ai/api/rest/v1";

    /// <summary>
    /// Таймаут для HTTP-запросов к Leonardo AI API в секундах.
    /// Leonardo может генерировать изображения дольше, чем DALL-E.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Интервал опроса статуса генерации в секундах.
    /// </summary>
    public int PollingIntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Максимальное количество попыток опроса статуса генерации.
    /// При интервале 5 секунд и 36 попытках - таймаут 3 минуты.
    /// </summary>
    public int MaxPollingAttempts { get; init; } = 36;

    /// <summary>
    /// Ширина генерируемого изображения в пикселях.
    /// </summary>
    public int ImageWidth { get; init; } = 1024;

    /// <summary>
    /// Высота генерируемого изображения в пикселях.
    /// </summary>
    public int ImageHeight { get; init; } = 1024;

    /// <summary>
    /// Guidance scale - баланс между креативностью и следованием промпту.
    /// Рекомендуемое значение 7-8 для ювелирных изделий.
    /// </summary>
    public int GuidanceScale { get; init; } = 8;

    /// <summary>
    /// Включить ли режим PhotoReal для фотореалистичных изображений.
    /// </summary>
    public bool PhotoReal { get; init; } = true;

    /// <summary>
    /// Включить ли режим Alchemy для улучшенного качества.
    /// </summary>
    public bool Alchemy { get; init; } = true;

    /// <summary>
    /// Негативный промпт для исключения нежелательных элементов.
    /// </summary>
    public string NegativePrompt { get; init; } = "blurry, low quality, distorted, unrealistic, bad proportions, deformed, artifacts, noise, watermark, text";
}
