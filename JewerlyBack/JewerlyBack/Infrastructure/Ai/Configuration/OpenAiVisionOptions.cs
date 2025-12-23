namespace JewerlyBack.Infrastructure.Ai.Configuration;

/// <summary>
/// Configuration options for OpenAI Vision API integration.
/// Used for jewelry image analysis in the upgrade flow.
/// </summary>
public sealed class OpenAiVisionOptions
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "OpenAiVision";

    /// <summary>
    /// OpenAI API key.
    /// IMPORTANT: Should be loaded from OPENAI_API_KEY environment variable, not stored in config.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL for OpenAI API.
    /// Default: https://api.openai.com/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Model to use for vision analysis.
    /// Default: gpt-4o (GPT-4 Omni with vision capabilities)
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Maximum tokens for the response.
    /// Default: 2048 (sufficient for structured analysis)
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Temperature for response generation.
    /// Lower values = more deterministic output.
    /// Default: 0.3 (low variance for consistent analysis)
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// HTTP timeout in seconds.
    /// Default: 60 seconds (vision analysis can be slow)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Image detail level for vision analysis.
    /// Options: "low", "high", "auto"
    /// Default: "high" (for fine jewelry details)
    /// </summary>
    public string ImageDetail { get; set; } = "high";

    /// <summary>
    /// Maximum retry attempts for transient failures.
    /// Default: 2
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>
    /// Delay between retries in milliseconds.
    /// Default: 2000 (2 seconds)
    /// </summary>
    public int RetryDelayMs { get; set; } = 2000;
}
