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
/// Реализация сервиса для генерации AI-изображений через OpenAI API.
/// </summary>
public sealed class OpenAiImageProvider : IAiImageProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly IS3StorageService _s3Storage;
    private readonly ILogger<OpenAiImageProvider> _logger;

    public OpenAiImageProvider(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        IS3StorageService s3Storage,
        ILogger<OpenAiImageProvider> logger)
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
        Console.WriteLine("│ 🖼️  SINGLE IMAGE GENERATION                                  │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine($"│ Job ID:          {jobId,-42}│");
        Console.WriteLine($"│ Configuration:   {configurationId,-42}│");
        Console.WriteLine($"│ Prompt Length:   {prompt.Length} characters{new string(' ', 30)}│");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");

        _logger.LogInformation(
            "🖼️ Starting single AI image generation. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
            configurationId, jobId);

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  [OpenAI] API Key NOT configured - using placeholder image");
            Console.WriteLine("   Set OPENAI_API_KEY environment variable for real AI generation");
            Console.WriteLine();

            _logger.LogWarning(
                "⚠️ OpenAI API key not configured. Returning placeholder image URL for development. " +
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
            // ===== STEP 1: Generate image via OpenAI API =====
            Console.WriteLine();
            Console.WriteLine("   📡 Step 1: Calling OpenAI API...");
            var apiStopwatch = Stopwatch.StartNew();

            var imageBytes = await GenerateImageBytesAsync(prompt, ct);

            apiStopwatch.Stop();
            Console.WriteLine($"   ✓ OpenAI API responded in {apiStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ Image size: {imageBytes.Length:N0} bytes ({imageBytes.Length / 1024.0:F1} KB)");

            // ===== STEP 2: Upload to S3 =====
            var fileKey = $"ai-previews/{configurationId}/{jobId}/preview.png";

            Console.WriteLine();
            Console.WriteLine("   💾 Step 2: Uploading to S3 storage...");
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
            Console.WriteLine("│ ✅ SINGLE IMAGE GENERATION COMPLETE                         │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Total time:    {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine($"│ API time:      {apiStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine($"│ Upload time:   {uploadStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogInformation(
                "✅ Single AI image generated successfully. JobId: {JobId}, URL: {Url}, TotalTime: {TotalTime}s, ApiTime: {ApiTime}s, UploadTime: {UploadTime}s",
                jobId, publicUrl, totalStopwatch.Elapsed.TotalSeconds, apiStopwatch.Elapsed.TotalSeconds, uploadStopwatch.Elapsed.TotalSeconds);

            return publicUrl;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ ❌ SINGLE IMAGE GENERATION FAILED                           │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Error Type:    {ex.GetType().Name,-44}│");
            var errorMsg = ex.Message.Length > 44 ? ex.Message[..41] + "..." : ex.Message;
            Console.WriteLine($"│ Error:         {errorMsg,-44}│");
            Console.WriteLine($"│ Time elapsed:  {totalStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 43)}│");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            _logger.LogError(ex,
                "❌ Failed to generate single AI image. ConfigurationId: {ConfigurationId}, JobId: {JobId}, Duration: {Duration}s",
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
            "Starting 360 AI preview generation. ConfigurationId: {ConfigurationId}, JobId: {JobId}, FrameCount: {FrameCount}",
            configurationId, jobId, frameCount);

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

                // Модифицируем промпт для каждого кадра, добавляя информацию об угле обзора
                var framePrompt = $"{prompt}, view angle {angle:F0} degrees around the jewelry piece, consistent lighting and style";

                _logger.LogDebug(
                    "Generating frame {FrameNumber}/{TotalFrames} at angle {Angle} degrees",
                    i + 1, frameCount, angle);

                // 1. Генерация изображения через OpenAI
                var imageBytes = await GenerateImageBytesAsync(framePrompt, ct);

                // 2. Формирование пути в S3 для кадра
                var fileKey = $"ai-previews/{configurationId}/{jobId}/frames/frame_{i:D2}.png";

                // 3. Загрузка в S3
                _logger.LogDebug("Uploading frame {FrameNumber} to S3: {FileKey}", i, fileKey);

                using var stream = new MemoryStream(imageBytes);
                var publicUrl = await _s3Storage.UploadAsync(stream, fileKey, "image/png", ct);

                frameUrls.Add(publicUrl);

                _logger.LogDebug(
                    "Frame {FrameNumber}/{TotalFrames} generated successfully",
                    i + 1, frameCount);

                // Небольшая задержка между запросами к API для избежания rate limiting
                if (i < frameCount - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
            }

            _logger.LogInformation(
                "360 AI preview generated successfully. JobId: {JobId}, FrameCount: {FrameCount}",
                jobId, frameUrls.Count);

            return frameUrls.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate 360 AI preview. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);
            throw;
        }
    }

    /// <summary>
    /// Выполняет запрос к OpenAI Images API и возвращает изображение в виде байтов.
    /// Используется как общий helper-метод для Single и 360 превью.
    /// </summary>
    private async Task<byte[]> GenerateImageBytesAsync(string prompt, CancellationToken ct)
    {
        // Формирование запроса к OpenAI API
        var request = new OpenAiImageRequest
        {
            Model = _options.Model,
            Prompt = prompt,
            N = 1,
            Size = "1024x1024",
            Quality = "standard",
            ResponseFormat = "b64_json" // Предпочитаем base64 для надежности
        };

        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        // Log HTTP request details
        Console.WriteLine();
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ 🌐 OpenAI API Request                                   │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │ Endpoint:     POST {_httpClient.BaseAddress}images/generations");
        Console.WriteLine($"   │ Model:        {_options.Model,-40}│");
        Console.WriteLine($"   │ Size:         1024x1024{new string(' ', 31)}│");
        Console.WriteLine($"   │ Quality:      standard{new string(' ', 32)}│");
        Console.WriteLine($"   │ Format:       b64_json{new string(' ', 32)}│");
        Console.WriteLine($"   │ Prompt size:  {prompt.Length} chars{new string(' ', 35)}│");
        Console.WriteLine("   │ Auth:         Bearer ***...*** (hidden){new string(' ', 15)}│");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        _logger.LogDebug("Sending request to OpenAI. Model={Model}, Size=1024x1024, PromptLength={PromptLength}",
            _options.Model, prompt.Length);

        var httpStopwatch = Stopwatch.StartNew();

        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        Console.WriteLine("   📤 Sending HTTP POST request to DALL-E...");
        Console.WriteLine($"   ⏱️  Waiting for DALL-E response (this may take 30-120 seconds)...");

        using var httpResponse = await _httpClient.PostAsync("images/generations", content, ct);

        httpStopwatch.Stop();

        // Log HTTP response details
        Console.WriteLine();
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ ✅ DALL-E CONNECTION SUCCESSFUL                         │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │ Response Time:  {httpStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 37)}│");
        Console.WriteLine($"   │ HTTP Status:    {(int)httpResponse.StatusCode} {httpResponse.StatusCode,-35}│");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        // Обработка ошибок
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);

            Console.WriteLine();
            Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("   │ ❌ OpenAI API Error Response                            │");
            Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
            Console.WriteLine($"   │ Status Code: {(int)httpResponse.StatusCode} {httpResponse.StatusCode,-34}│");
            Console.WriteLine("   │ Response Body:                                          │");

            // Print error body with line wrapping
            var errorLines = errorBody.Split('\n');
            foreach (var line in errorLines.Take(10))
            {
                var truncatedLine = line.Length > 55 ? line[..52] + "..." : line;
                Console.WriteLine($"   │ {truncatedLine,-55}│");
            }
            if (errorLines.Length > 10)
            {
                Console.WriteLine($"   │ ... ({errorLines.Length - 10} more lines)                              │");
            }
            Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

            _logger.LogError(
                "❌ OpenAI API returned error. Status: {StatusCode}, Body: {Body}",
                httpResponse.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"OpenAI API error: {httpResponse.StatusCode}. {errorBody}");
        }

        // Парсинг ответа
        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);

        Console.WriteLine($"   Response size: {responseJson.Length:N0} characters");

        _logger.LogDebug("Received response from OpenAI. ResponseLength={ResponseLength}", responseJson.Length);

        var response = JsonSerializer.Deserialize<OpenAiImageResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        if (response?.Data == null || response.Data.Count == 0)
        {
            Console.WriteLine("   ❌ OpenAI API returned empty data!");
            throw new InvalidOperationException("OpenAI API returned empty data");
        }

        Console.WriteLine($"   ✓ Received {response.Data.Count} image(s) in response");

        var imageData = response.Data[0];

        // Обработка base64 или URL
        if (!string.IsNullOrEmpty(imageData.B64Json))
        {
            Console.WriteLine("   📦 Decoding base64 image...");
            var imageBytes = Convert.FromBase64String(imageData.B64Json);
            Console.WriteLine($"   ✓ Decoded image: {imageBytes.Length:N0} bytes");
            _logger.LogDebug("Decoded base64 image. Size={Size} bytes", imageBytes.Length);
            return imageBytes;
        }
        else if (!string.IsNullOrEmpty(imageData.Url))
        {
            Console.WriteLine($"   📥 Downloading image from URL: {imageData.Url[..Math.Min(50, imageData.Url.Length)]}...");
            var downloadStopwatch = Stopwatch.StartNew();
            var imageBytes = await _httpClient.GetByteArrayAsync(imageData.Url, ct);
            downloadStopwatch.Stop();
            Console.WriteLine($"   ✓ Downloaded image: {imageBytes.Length:N0} bytes in {downloadStopwatch.Elapsed.TotalSeconds:F2}s");
            _logger.LogDebug("Downloaded image from URL. Size={Size} bytes, Duration={Duration}s",
                imageBytes.Length, downloadStopwatch.Elapsed.TotalSeconds);
            return imageBytes;
        }
        else
        {
            Console.WriteLine("   ❌ OpenAI API response doesn't contain b64_json or url!");
            throw new InvalidOperationException(
                "OpenAI API response doesn't contain b64_json or url");
        }
    }

    #region DTO Classes

    private sealed class OpenAiImageRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("n")]
        public int N { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;

        [JsonPropertyName("quality")]
        public string? Quality { get; set; }

        [JsonPropertyName("response_format")]
        public string ResponseFormat { get; set; } = string.Empty;
    }

    private sealed class OpenAiImageResponse
    {
        [JsonPropertyName("data")]
        public List<OpenAiImageData> Data { get; set; } = new();
    }

    private sealed class OpenAiImageData
    {
        [JsonPropertyName("b64_json")]
        public string? B64Json { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    #endregion
}
