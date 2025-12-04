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

        _logger.LogInformation(
            "Starting AI image generation. ConfigurationId: {ConfigurationId}, JobId: {JobId}, Prompt: {Prompt}",
            configurationId, jobId, prompt);

        try
        {
            // 1. Вызов OpenAI Images API
            var imageBytes = await GenerateImageWithOpenAiAsync(prompt, ct);

            // 2. Формирование пути в S3
            var fileKey = $"ai-previews/{configurationId}/{jobId}/preview.png";

            // 3. Загрузка в S3
            _logger.LogDebug("Uploading generated image to S3: {FileKey}", fileKey);

            using var stream = new MemoryStream(imageBytes);
            var publicUrl = await _s3Storage.UploadAsync(stream, fileKey, "image/png", ct);

            _logger.LogInformation(
                "AI image generated successfully. JobId: {JobId}, URL: {Url}",
                jobId, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate AI image. ConfigurationId: {ConfigurationId}, JobId: {JobId}",
                configurationId, jobId);
            throw;
        }
    }

    /// <summary>
    /// Выполняет запрос к OpenAI Images API и возвращает изображение в виде байтов.
    /// </summary>
    private async Task<byte[]> GenerateImageWithOpenAiAsync(string prompt, CancellationToken ct)
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

        _logger.LogDebug("Sending request to OpenAI: {Request}", requestJson);

        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        using var httpResponse = await _httpClient.PostAsync("/images/generations", content, ct);

        // Обработка ошибок
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "OpenAI API returned error. Status: {StatusCode}, Body: {Body}",
                httpResponse.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"OpenAI API error: {httpResponse.StatusCode}. {errorBody}");
        }

        // Парсинг ответа
        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        _logger.LogDebug("Received response from OpenAI: {Response}", responseJson);

        var response = JsonSerializer.Deserialize<OpenAiImageResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        if (response?.Data == null || response.Data.Count == 0)
        {
            throw new InvalidOperationException("OpenAI API returned empty data");
        }

        var imageData = response.Data[0];

        // Обработка base64 или URL
        if (!string.IsNullOrEmpty(imageData.B64Json))
        {
            _logger.LogDebug("Decoding base64 image");
            return Convert.FromBase64String(imageData.B64Json);
        }
        else if (!string.IsNullOrEmpty(imageData.Url))
        {
            _logger.LogDebug("Downloading image from URL: {Url}", imageData.Url);
            return await _httpClient.GetByteArrayAsync(imageData.Url, ct);
        }
        else
        {
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
