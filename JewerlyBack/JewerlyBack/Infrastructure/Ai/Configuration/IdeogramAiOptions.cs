namespace JewerlyBack.Infrastructure.Ai.Configuration;

/// <summary>
/// Configuration options for Ideogram AI API integration.
///
/// IMPORTANT: ApiKey must NOT be stored in appsettings.json!
/// ApiKey is automatically loaded from the IDEOGRAM_API_KEY environment variable.
///
/// Setting the key:
/// - Environment variable: IDEOGRAM_API_KEY=...
/// - Docker/Heroku/Render/GitHub Actions: set IDEOGRAM_API_KEY in environment variables
///
/// ApiKey validation occurs at application startup (ValidateOnStart).
/// If IDEOGRAM_API_KEY is not set in Production, the application will not start.
/// </summary>
public sealed class IdeogramAiOptions
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "Ai:Ideogram";

    /// <summary>
    /// API key for Ideogram AI API access.
    /// Loaded from the IDEOGRAM_API_KEY environment variable.
    /// Values from appsettings.json are ignored and overwritten at startup.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Ideogram AI API.
    /// Default: https://api.ideogram.ai
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.ideogram.ai";

    /// <summary>
    /// API path for image generation.
    /// For Ideogram 3.0: /v1/ideogram-v3/generate
    /// </summary>
    public string GenerateEndpoint { get; init; } = "/v1/ideogram-v3/generate";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// Ideogram API is synchronous (no polling needed), typical generation 5-15 seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Aspect ratio for generated images.
    /// Format: "1x1" for 1:1 square, "16x9", "9x16", "4x3", "3x4", etc.
    /// </summary>
    public string AspectRatio { get; init; } = "1x1";

    /// <summary>
    /// Rendering speed option.
    /// Values: "FLASH" (fastest), "TURBO", "DEFAULT", "QUALITY" (slowest, best quality)
    /// </summary>
    public string RenderingSpeed { get; init; } = "DEFAULT";

    /// <summary>
    /// Magic prompt option - enhances the prompt automatically.
    /// Values: true or false
    /// </summary>
    public bool MagicPrompt { get; init; } = true;

    /// <summary>
    /// Style type for generation.
    /// Values: "DESIGN", "REALISTIC", "GENERAL", "RENDER_3D", "ANIME"
    /// "DESIGN" is recommended for jewelry/product visualization.
    /// </summary>
    public string StyleType { get; init; } = "DESIGN";

    /// <summary>
    /// Negative prompt to exclude unwanted elements.
    /// Ensures pure white background for jewelry visualization.
    /// </summary>
    public string NegativePrompt { get; init; } = "blurry, low quality, distorted, unrealistic, bad proportions, deformed, artifacts, noise, watermark, text, colored background, textured background, gradient background, gray background, dark background, shadows on background, reflections on background, patterned background, busy background";
}
