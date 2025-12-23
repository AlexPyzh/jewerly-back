using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using JewerlyBack.Application.Ai;
using JewerlyBack.Infrastructure.Ai.Configuration;
using JewerlyBack.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace JewerlyBack.Infrastructure.Ai;

/// <summary>
/// Implementation of AI image generation service using direct Ideogram API.
/// Ideogram 3.0 API is synchronous - no polling required.
/// Uses multipart/form-data for requests.
/// Features:
/// - Direct API calls with immediate response
/// - HttpClientFactory for connection pooling
/// - Streaming image transfer to S3
/// </summary>
public sealed class IdeogramAiImageProvider : IAiImageProvider
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdeogramAiOptions _options;
    private readonly IS3StorageService _s3Storage;
    private readonly ILogger<IdeogramAiImageProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public IdeogramAiImageProvider(
        HttpClient httpClient,
        IHttpClientFactory httpClientFactory,
        IOptions<IdeogramAiOptions> options,
        IS3StorageService s3Storage,
        ILogger<IdeogramAiImageProvider> logger)
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _s3Storage = s3Storage;
        _logger = logger;
    }

    /// <summary>
    /// Generates a single preview image based on the prompt.
    /// </summary>
    public async Task<string> GenerateSinglePreviewAsync(
        string prompt,
        Guid configurationId,
        Guid jobId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        Console.WriteLine();
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 🖼️  SINGLE IMAGE GENERATION (Ideogram AI 3.0)               │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine($"│ Job ID:          {jobId,-42}│");
        Console.WriteLine($"│ Configuration:   {configurationId,-42}│");
        Console.WriteLine($"│ Prompt Length:   {prompt.Length} characters{new string(' ', 30)}│");
        Console.WriteLine($"│ Aspect Ratio:    {_options.AspectRatio,-42}│");
        Console.WriteLine($"│ Rendering Speed: {_options.RenderingSpeed,-42}│");
        Console.WriteLine($"│ Style Type:      {_options.StyleType,-42}│");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");

        _logger.LogInformation(
            "🖼️ Starting single AI image generation with Ideogram AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
            configurationId, jobId);

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  [Ideogram AI] API Key NOT configured - using placeholder image");
            Console.WriteLine("   Set IDEOGRAM_API_KEY environment variable for real AI generation");
            Console.WriteLine();

            _logger.LogWarning(
                "⚠️ Ideogram AI API key not configured. Returning placeholder image URL for development. " +
                "ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);

            // Return a placeholder image URL for development
            var placeholderUrl = "https://via.placeholder.com/1024x1024/DAA520/FFFFFF?text=AI+Preview+Placeholder";
            Console.WriteLine($"   Placeholder URL: {placeholderUrl}");
            return placeholderUrl;
        }

        var totalStopwatch = Stopwatch.StartNew();

        try
        {
            // ===== STEP 1: Generate image via Ideogram API (synchronous) =====
            Console.WriteLine();
            Console.WriteLine("   📡 Step 1: Generating image with Ideogram AI 3.0...");
            var apiStopwatch = Stopwatch.StartNew();

            var ideogramImageUrl = await GenerateImageAsync(prompt, ct);

            apiStopwatch.Stop();
            Console.WriteLine($"   ✓ Ideogram AI generation completed in {apiStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ Ideogram image URL: {ideogramImageUrl[..Math.Min(60, ideogramImageUrl.Length)]}...");

            // ===== STEP 2: Download from Ideogram and Upload to S3 =====
            var fileKey = $"ai-previews/{configurationId}/{jobId}/preview.png";

            Console.WriteLine();
            Console.WriteLine("   📥💾 Step 2: Download from Ideogram & Upload to S3...");
            var transferStopwatch = Stopwatch.StartNew();

            var publicUrl = await DownloadAndUploadToS3Async(ideogramImageUrl, fileKey, ct);

            transferStopwatch.Stop();
            totalStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ ✅ SINGLE IMAGE GENERATION COMPLETE (Ideogram AI)          │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Total time:        {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 39)}│");
            Console.WriteLine($"│ API generation:    {apiStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 39)}│");
            Console.WriteLine($"│ Transfer (DL+UL):  {transferStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 39)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogInformation(
                "✅ Single AI image generated successfully with Ideogram AI. JobId: {JobId}, URL: {Url}, TotalTime: {TotalTime}s, GenerationTime: {GenTime}s, TransferTime: {TransferTime}s",
                jobId, publicUrl, totalStopwatch.Elapsed.TotalSeconds, apiStopwatch.Elapsed.TotalSeconds,
                transferStopwatch.Elapsed.TotalSeconds);

            return publicUrl;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ ❌ SINGLE IMAGE GENERATION FAILED (Ideogram AI)            │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Error Type:    {ex.GetType().Name,-44}│");
            var errorMsg = ex.Message.Length > 44 ? ex.Message[..41] + "..." : ex.Message;
            Console.WriteLine($"│ Error:         {errorMsg,-44}│");
            Console.WriteLine($"│ Time elapsed:  {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogError(ex,
                "❌ Failed to generate single AI image with Ideogram AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}, Duration: {Duration}s",
                configurationId, jobId, totalStopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }

    /// <summary>
    /// Generates a set of images for 360-degree preview.
    /// </summary>
    public async Task<IReadOnlyList<string>> Generate360PreviewAsync(
        string prompt,
        Guid configurationId,
        Guid jobId,
        int frameCount = 12,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        if (frameCount < 4 || frameCount > 36)
        {
            throw new ArgumentException("Frame count must be between 4 and 36", nameof(frameCount));
        }

        _logger.LogInformation(
            "Starting 360 AI preview generation with Ideogram AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}, FrameCount: {FrameCount}",
            configurationId, jobId, frameCount);

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "⚠️ Ideogram AI API key not configured. Returning placeholder URLs for development. " +
                "ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);

            var placeholderUrls = Enumerable
                .Range(0, frameCount)
                .Select(i => $"https://via.placeholder.com/1024x1024/DAA520/FFFFFF?text=Frame+{i:D2}")
                .ToList();
            return placeholderUrls.AsReadOnly();
        }

        try
        {
            var frameUrls = new List<string>(frameCount);
            var angleStep = 360.0 / frameCount;

            for (int i = 0; i < frameCount; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "360 preview generation cancelled at frame {CurrentFrame}/{TotalFrames}",
                        i, frameCount);
                    ct.ThrowIfCancellationRequested();
                }

                var angle = i * angleStep;

                // Modify prompt for each frame with view angle information
                var framePrompt = $"{prompt}, view angle {angle:F0} degrees around the jewelry piece, consistent lighting and style";

                _logger.LogDebug(
                    "Generating frame {FrameNumber}/{TotalFrames} at angle {Angle} degrees",
                    i + 1, frameCount, angle);

                // 1. Generate image with Ideogram (synchronous)
                var ideogramImageUrl = await GenerateImageAsync(framePrompt, ct);

                // 2. Download and upload to S3
                var fileKey = $"ai-previews/{configurationId}/{jobId}/frames/frame_{i:D2}.png";

                _logger.LogDebug("Uploading frame {FrameNumber} to S3: {FileKey}", i, fileKey);

                var publicUrl = await DownloadAndUploadToS3Async(ideogramImageUrl, fileKey, ct);

                frameUrls.Add(publicUrl);

                _logger.LogDebug(
                    "Frame {FrameNumber}/{TotalFrames} generated successfully",
                    i + 1, frameCount);

                // Small delay between requests to avoid rate limiting
                if (i < frameCount - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }

            _logger.LogInformation(
                "360 AI preview generated successfully with Ideogram AI. JobId: {JobId}, FrameCount: {FrameCount}",
                jobId, frameUrls.Count);

            return frameUrls.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate 360 AI preview with Ideogram AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);
            throw;
        }
    }

    /// <summary>
    /// Generates an image using the Ideogram 3.0 API.
    /// Uses multipart/form-data format as required by the API.
    /// Ideogram API is synchronous - returns image URL immediately in response.
    /// </summary>
    private async Task<string> GenerateImageAsync(string prompt, CancellationToken ct)
    {
        var endpoint = _options.GenerateEndpoint.TrimStart('/');
        var fullUrl = $"{_options.BaseUrl.TrimEnd('/')}/{endpoint}";

        Console.WriteLine();
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ 🌐 Ideogram AI 3.0 API Request                          │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │ Endpoint:      POST {fullUrl}");
        Console.WriteLine($"   │ Aspect Ratio:  {_options.AspectRatio,-40}│");
        Console.WriteLine($"   │ Speed:         {_options.RenderingSpeed,-40}│");
        Console.WriteLine($"   │ Style Type:    {_options.StyleType,-40}│");
        Console.WriteLine($"   │ Magic Prompt:  {_options.MagicPrompt,-40}│");
        Console.WriteLine($"   │ Prompt size:   {prompt.Length} chars{new string(' ', 35)}│");
        Console.WriteLine("   │ Auth:          Api-Key ***...*** (hidden){new string(' ', 12)}│");
        Console.WriteLine("   │ Content-Type:  multipart/form-data{new string(' ', 20)}│");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        _logger.LogDebug(
            "Sending generation request to Ideogram AI 3.0. AspectRatio={AspectRatio}, Speed={Speed}, PromptLength={PromptLength}",
            _options.AspectRatio, _options.RenderingSpeed, prompt.Length);

        // Create multipart form data content
        using var formContent = new MultipartFormDataContent();

        // Add required prompt field
        formContent.Add(new StringContent(prompt), "prompt");

        // Add aspect ratio
        formContent.Add(new StringContent(_options.AspectRatio), "aspect_ratio");

        // Add rendering speed
        formContent.Add(new StringContent(_options.RenderingSpeed), "rendering_speed");

        // Add style type
        if (!string.IsNullOrWhiteSpace(_options.StyleType))
        {
            formContent.Add(new StringContent(_options.StyleType), "style_type");
        }

        // Add negative prompt if configured
        if (!string.IsNullOrWhiteSpace(_options.NegativePrompt))
        {
            formContent.Add(new StringContent(_options.NegativePrompt), "negative_prompt");
        }

        Console.WriteLine("   📤 Sending HTTP POST request to Ideogram AI...");

        // Make the request to the full endpoint URL
        using var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
        request.Content = formContent;

        // Add API key header
        request.Headers.Add("Api-Key", _options.ApiKey);

        using var httpResponse = await _httpClient.SendAsync(request, ct);

        // Handle errors
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);

            Console.WriteLine();
            Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("   │ ❌ Ideogram AI API Error Response                       │");
            Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
            Console.WriteLine($"   │ Status Code: {(int)httpResponse.StatusCode} {httpResponse.StatusCode,-34}│");
            Console.WriteLine("   │ Response Body:                                          │");

            var errorLines = errorBody.Split('\n');
            foreach (var line in errorLines.Take(10))
            {
                var truncatedLine = line.Length > 55 ? line[..52] + "..." : line;
                Console.WriteLine($"   │ {truncatedLine,-55}│");
            }
            Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

            _logger.LogError(
                "❌ Ideogram AI API returned error. Status: {StatusCode}, Body: {Body}",
                httpResponse.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"Ideogram AI API error: {httpResponse.StatusCode}. {errorBody}");
        }

        // Parse response
        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);

        Console.WriteLine($"   Response received: {responseJson[..Math.Min(200, responseJson.Length)]}...");

        var response = JsonSerializer.Deserialize<IdeogramGenerationResponse>(responseJson, JsonOptions);

        if (response?.Data == null || response.Data.Count == 0)
        {
            Console.WriteLine("   ❌ Ideogram AI API returned empty response!");
            throw new InvalidOperationException("Ideogram AI API returned empty response");
        }

        var imageUrl = response.Data[0].Url;

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            Console.WriteLine("   ❌ Ideogram AI API returned empty image URL!");
            throw new InvalidOperationException("Ideogram AI API returned empty image URL");
        }

        Console.WriteLine($"   ✓ Image generated successfully!");
        return imageUrl;
    }

    /// <summary>
    /// Downloads image from Ideogram and uploads directly to S3 using streaming.
    /// This is more memory-efficient than loading entire image into byte array.
    /// Returns the public S3 URL.
    /// </summary>
    private async Task<string> DownloadAndUploadToS3Async(
        string ideogramImageUrl,
        string s3FileKey,
        CancellationToken ct)
    {
        Console.WriteLine($"   Streaming from Ideogram to S3...");
        Console.WriteLine($"   Source: {ideogramImageUrl[..Math.Min(60, ideogramImageUrl.Length)]}...");
        Console.WriteLine($"   Target: {s3FileKey}");

        var transferStopwatch = Stopwatch.StartNew();

        using var downloadClient = _httpClientFactory.CreateClient("IdeogramImageDownload");

        // Get the response stream without buffering entire content
        using var response = await downloadClient.GetAsync(ideogramImageUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        Console.WriteLine($"   Content-Length: {(contentLength.HasValue ? $"{contentLength:N0} bytes" : "unknown")}");

        // For S3 upload, we need to buffer since AWS SDK needs seekable stream or known length
        await using var sourceStream = await response.Content.ReadAsStreamAsync(ct);

        // Buffer into memory stream (required for S3 upload with Contabo)
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        if (memoryStream.Length == 0)
        {
            throw new InvalidOperationException("Downloaded image is empty");
        }

        Console.WriteLine($"   Downloaded {memoryStream.Length:N0} bytes ({memoryStream.Length / 1024.0:F1} KB)");

        // Upload to S3
        var publicUrl = await _s3Storage.UploadAsync(memoryStream, s3FileKey, "image/png", ct);

        transferStopwatch.Stop();
        Console.WriteLine($"   ✓ Transfer complete in {transferStopwatch.Elapsed.TotalSeconds:F2}s");

        return publicUrl;
    }

    #region DTO Classes

    /// <summary>
    /// Ideogram API response structure.
    /// Response is synchronous - contains image URL directly.
    /// </summary>
    private sealed class IdeogramGenerationResponse
    {
        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("data")]
        public List<IdeogramImageData>? Data { get; set; }
    }

    private sealed class IdeogramImageData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        [JsonPropertyName("resolution")]
        public string? Resolution { get; set; }

        [JsonPropertyName("is_image_safe")]
        public bool? IsImageSafe { get; set; }

        [JsonPropertyName("seed")]
        public int? Seed { get; set; }
    }

    #endregion
}
