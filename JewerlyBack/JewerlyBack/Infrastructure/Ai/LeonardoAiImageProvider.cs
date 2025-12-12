using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JewerlyBack.Application.Ai;
using JewerlyBack.Infrastructure.Ai.Configuration;
using JewerlyBack.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace JewerlyBack.Infrastructure.Ai;

/// <summary>
/// Реализация сервиса для генерации AI-изображений через Leonardo AI API.
/// </summary>
public sealed class LeonardoAiImageProvider : IAiImageProvider
{
    private readonly HttpClient _httpClient;
    private readonly LeonardoAiOptions _options;
    private readonly IS3StorageService _s3Storage;
    private readonly ILogger<LeonardoAiImageProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LeonardoAiImageProvider(
        HttpClient httpClient,
        IOptions<LeonardoAiOptions> options,
        IS3StorageService s3Storage,
        ILogger<LeonardoAiImageProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _s3Storage = s3Storage;
        _logger = logger;
    }

    /// <summary>
    /// Генерирует одиночное превью изображение на основе промпта.
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
        Console.WriteLine("│ 🖼️  SINGLE IMAGE GENERATION (Leonardo AI)                   │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine($"│ Job ID:          {jobId,-42}│");
        Console.WriteLine($"│ Configuration:   {configurationId,-42}│");
        Console.WriteLine($"│ Prompt Length:   {prompt.Length} characters{new string(' ', 30)}│");
        Console.WriteLine($"│ Model ID:        {_options.ModelId[..Math.Min(42, _options.ModelId.Length)],-42}│");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");

        _logger.LogInformation(
            "🖼️ Starting single AI image generation with Leonardo AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
            configurationId, jobId);

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  [Leonardo AI] API Key NOT configured - using placeholder image");
            Console.WriteLine("   Set LEONARDO_API_KEY environment variable for real AI generation");
            Console.WriteLine();

            _logger.LogWarning(
                "⚠️ Leonardo AI API key not configured. Returning placeholder image URL for development. " +
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
            // ===== STEP 1: Start generation via Leonardo AI API =====
            Console.WriteLine();
            Console.WriteLine("   📡 Step 1: Starting Leonardo AI generation...");
            var apiStopwatch = Stopwatch.StartNew();

            var generationId = await StartGenerationAsync(prompt, ct);

            Console.WriteLine($"   ✓ Generation started with ID: {generationId}");
            _logger.LogInformation("Leonardo generation started with ID: {GenerationId}", generationId);

            // ===== STEP 2: Poll for generation completion =====
            Console.WriteLine();
            Console.WriteLine("   ⏳ Step 2: Waiting for generation to complete...");

            var leonardoImageUrl = await PollForCompletionAsync(generationId, ct);

            apiStopwatch.Stop();
            Console.WriteLine($"   ✓ Leonardo AI generation completed in {apiStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ Leonardo image URL: {leonardoImageUrl[..Math.Min(60, leonardoImageUrl.Length)]}...");

            // ===== STEP 3: Download image from Leonardo =====
            Console.WriteLine();
            Console.WriteLine("   📥 Step 3: Downloading image from Leonardo...");
            var downloadStopwatch = Stopwatch.StartNew();

            var imageBytes = await DownloadImageAsync(leonardoImageUrl, ct);

            downloadStopwatch.Stop();
            Console.WriteLine($"   ✓ Downloaded in {downloadStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ Image size: {imageBytes.Length:N0} bytes ({imageBytes.Length / 1024.0:F1} KB)");

            // ===== STEP 4: Upload to S3 =====
            var fileKey = $"ai-previews/{configurationId}/{jobId}/preview.png";

            Console.WriteLine();
            Console.WriteLine("   💾 Step 4: Uploading to S3 storage...");
            Console.WriteLine($"   Target bucket key: {fileKey}");
            Console.WriteLine($"   Content type: image/png");
            Console.WriteLine($"   File size: {imageBytes.Length:N0} bytes");

            var uploadStopwatch = Stopwatch.StartNew();

            using var stream = new MemoryStream(imageBytes);
            var publicUrl = await _s3Storage.UploadAsync(stream, fileKey, "image/png", ct);

            uploadStopwatch.Stop();
            Console.WriteLine($"   ✓ Upload completed in {uploadStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ Public URL: {publicUrl}");

            totalStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ ✅ SINGLE IMAGE GENERATION COMPLETE (Leonardo AI)          │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Total time:      {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 41)}│");
            Console.WriteLine($"│ Generation time: {apiStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 41)}│");
            Console.WriteLine($"│ Download time:   {downloadStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 41)}│");
            Console.WriteLine($"│ Upload time:     {uploadStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 41)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogInformation(
                "✅ Single AI image generated successfully with Leonardo AI. JobId: {JobId}, URL: {Url}, TotalTime: {TotalTime}s, GenerationTime: {GenTime}s, DownloadTime: {DownloadTime}s, UploadTime: {UploadTime}s",
                jobId, publicUrl, totalStopwatch.Elapsed.TotalSeconds, apiStopwatch.Elapsed.TotalSeconds,
                downloadStopwatch.Elapsed.TotalSeconds, uploadStopwatch.Elapsed.TotalSeconds);

            return publicUrl;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ ❌ SINGLE IMAGE GENERATION FAILED (Leonardo AI)            │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Error Type:    {ex.GetType().Name,-44}│");
            var errorMsg = ex.Message.Length > 44 ? ex.Message[..41] + "..." : ex.Message;
            Console.WriteLine($"│ Error:         {errorMsg,-44}│");
            Console.WriteLine($"│ Time elapsed:  {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogError(ex,
                "❌ Failed to generate single AI image with Leonardo AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}, Duration: {Duration}s",
                configurationId, jobId, totalStopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }

    /// <summary>
    /// Генерирует набор изображений для 360-градусного превью.
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
            "Starting 360 AI preview generation with Leonardo AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}, FrameCount: {FrameCount}",
            configurationId, jobId, frameCount);

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "⚠️ Leonardo AI API key not configured. Returning placeholder URLs for development. " +
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

                // 1. Start generation
                var generationId = await StartGenerationAsync(framePrompt, ct);

                // 2. Poll for completion
                var leonardoImageUrl = await PollForCompletionAsync(generationId, ct);

                // 3. Download image from Leonardo
                var imageBytes = await DownloadImageAsync(leonardoImageUrl, ct);

                // 4. Upload to S3
                var fileKey = $"ai-previews/{configurationId}/{jobId}/frames/frame_{i:D2}.png";

                _logger.LogDebug("Uploading frame {FrameNumber} to S3: {FileKey}", i, fileKey);

                using var stream = new MemoryStream(imageBytes);
                var publicUrl = await _s3Storage.UploadAsync(stream, fileKey, "image/png", ct);

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
                "360 AI preview generated successfully with Leonardo AI. JobId: {JobId}, FrameCount: {FrameCount}",
                jobId, frameUrls.Count);

            return frameUrls.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate 360 AI preview with Leonardo AI. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);
            throw;
        }
    }

    /// <summary>
    /// Starts a new image generation job with Leonardo AI.
    /// Returns the generation ID for polling.
    /// </summary>
    private async Task<string> StartGenerationAsync(string prompt, CancellationToken ct)
    {
        // PhotoReal V1: does NOT accept modelId (uses automatic model)
        // PhotoReal V2: REQUIRES modelId to be specified
        // No PhotoReal: uses modelId directly
        var usePhotoReal = _options.PhotoReal;

        var request = new LeonardoGenerationRequest
        {
            Prompt = prompt,
            ModelId = _options.ModelId, // Always send modelId - required for PhotoReal V2 and non-PhotoReal modes
            Width = _options.ImageWidth,
            Height = _options.ImageHeight,
            NumImages = 1,
            GuidanceScale = _options.GuidanceScale,
            PhotoReal = usePhotoReal ? true : null, // Only send if enabled
            PhotoRealVersion = usePhotoReal ? "v2" : null, // Use PhotoReal V2 for better quality
            Alchemy = _options.Alchemy ? true : null, // Only send if enabled
            NegativePrompt = _options.NegativePrompt
        };

        var requestJson = JsonSerializer.Serialize(request, JsonOptions);

        var modelDisplay = usePhotoReal ? $"PhotoReal V2 + {_options.ModelId}" : _options.ModelId;
        Console.WriteLine();
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ 🌐 Leonardo AI API Request                              │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │ Endpoint:     POST {_httpClient.BaseAddress}generations");
        Console.WriteLine($"   │ Model:        {modelDisplay[..Math.Min(40, modelDisplay.Length)],-40}│");
        Console.WriteLine($"   │ Size:         {_options.ImageWidth}x{_options.ImageHeight}{new string(' ', 33)}│");
        Console.WriteLine($"   │ Guidance:     {_options.GuidanceScale,-41}│");
        Console.WriteLine($"   │ PhotoReal:    {_options.PhotoReal,-41}│");
        Console.WriteLine($"   │ Alchemy:      {_options.Alchemy,-41}│");
        Console.WriteLine($"   │ Prompt size:  {prompt.Length} chars{new string(' ', 35)}│");
        Console.WriteLine("   │ Auth:         Bearer ***...*** (hidden){new string(' ', 15)}│");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        _logger.LogDebug("Sending generation request to Leonardo AI. Model={Model}, Size={Width}x{Height}, PromptLength={PromptLength}, PhotoReal={PhotoReal}",
            modelDisplay, _options.ImageWidth, _options.ImageHeight, prompt.Length, usePhotoReal);

        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        Console.WriteLine("   📤 Sending HTTP POST request to Leonardo AI...");

        using var httpResponse = await _httpClient.PostAsync("generations", content, ct);

        // Handle errors
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);

            Console.WriteLine();
            Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("   │ ❌ Leonardo AI API Error Response                       │");
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
                "❌ Leonardo AI API returned error. Status: {StatusCode}, Body: {Body}",
                httpResponse.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"Leonardo AI API error: {httpResponse.StatusCode}. {errorBody}");
        }

        // Parse response
        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        var response = JsonSerializer.Deserialize<LeonardoGenerationStartResponse>(responseJson, JsonOptions);

        if (response?.SdGenerationJob?.GenerationId == null)
        {
            Console.WriteLine("   ❌ Leonardo AI API returned empty generation ID!");
            throw new InvalidOperationException("Leonardo AI API returned empty generation ID");
        }

        return response.SdGenerationJob.GenerationId;
    }

    /// <summary>
    /// Polls Leonardo AI API for generation completion.
    /// Returns the generated image URL when ready.
    /// </summary>
    private async Task<string> PollForCompletionAsync(string generationId, CancellationToken ct)
    {
        var pollingInterval = TimeSpan.FromSeconds(_options.PollingIntervalSeconds);
        var maxAttempts = _options.MaxPollingAttempts;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            Console.WriteLine($"   ⏳ Polling attempt {attempt}/{maxAttempts}...");
            _logger.LogDebug("Polling Leonardo AI generation status. GenerationId={GenerationId}, Attempt={Attempt}/{MaxAttempts}",
                generationId, attempt, maxAttempts);

            using var httpResponse = await _httpClient.GetAsync($"generations/{generationId}", ct);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Leonardo AI status poll failed. Status: {StatusCode}, Body: {Body}",
                    httpResponse.StatusCode, errorBody);

                // Continue polling on non-critical errors
                if ((int)httpResponse.StatusCode >= 500)
                {
                    await Task.Delay(pollingInterval, ct);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Leonardo AI API error while polling: {httpResponse.StatusCode}. {errorBody}");
            }

            var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
            var response = JsonSerializer.Deserialize<LeonardoGenerationStatusResponse>(responseJson, JsonOptions);

            var status = response?.GenerationsByPk?.Status;

            Console.WriteLine($"   Status: {status ?? "unknown"}");

            switch (status?.ToUpperInvariant())
            {
                case "PENDING":
                case "PROCESSING":
                    // Still processing, wait and continue polling
                    await Task.Delay(pollingInterval, ct);
                    continue;

                case "COMPLETE":
                    // Generation complete, extract image URL
                    var generatedImages = response?.GenerationsByPk?.GeneratedImages;
                    if (generatedImages == null || generatedImages.Count == 0)
                    {
                        throw new InvalidOperationException("Leonardo AI generation completed but no images were returned");
                    }

                    var imageUrl = generatedImages[0].Url;
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        throw new InvalidOperationException("Leonardo AI generation completed but image URL is empty");
                    }

                    Console.WriteLine($"   ✓ Generation complete!");
                    return imageUrl;

                case "FAILED":
                    var errorMessage = response?.GenerationsByPk?.GeneratedImages?.FirstOrDefault()?.Url ?? "Unknown error";
                    throw new InvalidOperationException($"Leonardo AI generation failed: {errorMessage}");

                default:
                    _logger.LogWarning("Unknown Leonardo AI generation status: {Status}", status);
                    await Task.Delay(pollingInterval, ct);
                    continue;
            }
        }

        throw new TimeoutException($"Leonardo AI generation timed out after {maxAttempts * _options.PollingIntervalSeconds} seconds");
    }

    /// <summary>
    /// Downloads an image from Leonardo AI's temporary URL.
    /// </summary>
    private async Task<byte[]> DownloadImageAsync(string imageUrl, CancellationToken ct)
    {
        Console.WriteLine($"   Downloading from: {imageUrl[..Math.Min(60, imageUrl.Length)]}...");

        // Use a separate HttpClient without auth headers for downloading
        using var downloadClient = new HttpClient();
        downloadClient.Timeout = TimeSpan.FromSeconds(60);

        var imageBytes = await downloadClient.GetByteArrayAsync(imageUrl, ct);

        if (imageBytes.Length == 0)
        {
            throw new InvalidOperationException("Downloaded image is empty");
        }

        return imageBytes;
    }

    #region DTO Classes

    private sealed class LeonardoGenerationRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Model ID - required for PhotoReal V2 and standard generation modes.
        /// PhotoReal V2 compatible models:
        /// - Leonardo Kino XL: aa77f04e-3eec-4034-9c07-d0f619684628
        /// - Leonardo Vision XL: 5c232a9e-9061-4777-980a-ddc8e65647c6
        /// - Leonardo Diffusion XL: 1e60896f-3c26-4296-8ecc-53e2afecc132
        /// </summary>
        [JsonPropertyName("modelId")]
        public string ModelId { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("num_images")]
        public int NumImages { get; set; }

        [JsonPropertyName("guidance_scale")]
        public int? GuidanceScale { get; set; }

        [JsonPropertyName("photoReal")]
        public bool? PhotoReal { get; set; }

        [JsonPropertyName("alchemy")]
        public bool? Alchemy { get; set; }

        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; set; }

        /// <summary>
        /// PhotoReal version. Use "v2" for PhotoReal V2.
        /// </summary>
        [JsonPropertyName("photoRealVersion")]
        public string? PhotoRealVersion { get; set; }
    }

    private sealed class LeonardoGenerationStartResponse
    {
        [JsonPropertyName("sdGenerationJob")]
        public SdGenerationJob? SdGenerationJob { get; set; }
    }

    private sealed class SdGenerationJob
    {
        [JsonPropertyName("generationId")]
        public string? GenerationId { get; set; }
    }

    private sealed class LeonardoGenerationStatusResponse
    {
        [JsonPropertyName("generations_by_pk")]
        public GenerationsByPk? GenerationsByPk { get; set; }
    }

    private sealed class GenerationsByPk
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("generated_images")]
        public List<GeneratedImage>? GeneratedImages { get; set; }
    }

    private sealed class GeneratedImage
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    #endregion
}
