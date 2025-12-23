using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JewerlyBack.Application.Ai;
using JewerlyBack.Infrastructure.Ai.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JewerlyBack.Infrastructure.Ai;

/// <summary>
/// OpenAI Vision API client for jewelry image analysis.
/// Implements IJewelryVisionAnalyzer using GPT-4o with vision capabilities.
/// </summary>
public class OpenAiVisionClient : IJewelryVisionAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiVisionOptions _options;
    private readonly IJewelryAnalysisPromptBuilder _promptBuilder;
    private readonly ILogger<OpenAiVisionClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenAiVisionClient(
        HttpClient httpClient,
        IOptions<OpenAiVisionOptions> options,
        IJewelryAnalysisPromptBuilder promptBuilder,
        ILogger<OpenAiVisionClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task<JewelryAnalysisResponse> AnalyzeJewelryImageAsync(
        string imageUrl,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting jewelry analysis for image URL");

        var imageContent = new OpenAiImageContent
        {
            Type = "image_url",
            ImageUrl = new OpenAiImageUrl
            {
                Url = imageUrl,
                Detail = _options.ImageDetail
            }
        };

        return await PerformAnalysisAsync(imageContent, ct);
    }

    public async Task<JewelryAnalysisResponse> AnalyzeJewelryImageFromBase64Async(
        string base64Image,
        string mimeType,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting jewelry analysis for base64 image");

        var dataUrl = $"data:{mimeType};base64,{base64Image}";
        var imageContent = new OpenAiImageContent
        {
            Type = "image_url",
            ImageUrl = new OpenAiImageUrl
            {
                Url = dataUrl,
                Detail = _options.ImageDetail
            }
        };

        return await PerformAnalysisAsync(imageContent, ct);
    }

    private async Task<JewelryAnalysisResponse> PerformAnalysisAsync(
        OpenAiImageContent imageContent,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("OpenAI API key is not configured");
            return CreateErrorResponse("Vision analysis is not available. API key not configured.");
        }

        var request = BuildChatCompletionRequest(imageContent);
        var retryCount = 0;

        while (retryCount <= _options.MaxRetries)
        {
            try
            {
                var response = await SendRequestAsync(request, ct);
                return ParseResponse(response);
            }
            catch (HttpRequestException ex) when (retryCount < _options.MaxRetries)
            {
                retryCount++;
                _logger.LogWarning(ex,
                    "OpenAI API request failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms",
                    retryCount, _options.MaxRetries + 1, _options.RetryDelayMs);

                await Task.Delay(_options.RetryDelayMs, ct);
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Jewelry vision analysis failed");
                return CreateErrorResponse("Analysis could not be completed. Please try again.");
            }
        }

        return CreateErrorResponse("Analysis service temporarily unavailable. Please try again shortly.");
    }

    private OpenAiChatCompletionRequest BuildChatCompletionRequest(OpenAiImageContent imageContent)
    {
        return new OpenAiChatCompletionRequest
        {
            Model = _options.Model,
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature,
            ResponseFormat = new OpenAiResponseFormat { Type = "json_object" },
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = "system",
                    Content = _promptBuilder.GetSystemPrompt()
                },
                new OpenAiMessage
                {
                    Role = "user",
                    Content = new List<object>
                    {
                        imageContent,
                        new OpenAiTextContent
                        {
                            Type = "text",
                            Text = _promptBuilder.GetUserMessage()
                        }
                    }
                }
            }
        };
    }

    private async Task<string> SendRequestAsync(OpenAiChatCompletionRequest request, CancellationToken ct)
    {
        var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = httpContent
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        _logger.LogDebug("Sending request to OpenAI Vision API");

        var response = await _httpClient.SendAsync(httpRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenAI API error: {StatusCode} - {Body}",
                response.StatusCode, errorBody);

            throw new HttpRequestException(
                $"OpenAI API returned {response.StatusCode}: {errorBody}");
        }

        return await response.Content.ReadAsStringAsync(ct);
    }

    private JewelryAnalysisResponse ParseResponse(string responseJson)
    {
        try
        {
            var apiResponse = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(
                responseJson, _jsonOptions);

            if (apiResponse?.Choices == null || apiResponse.Choices.Count == 0)
            {
                _logger.LogWarning("OpenAI response contained no choices");
                return CreateErrorResponse("Analysis returned no results.");
            }

            var messageContent = apiResponse.Choices[0].Message?.Content;
            if (string.IsNullOrEmpty(messageContent))
            {
                _logger.LogWarning("OpenAI response message content is empty");
                return CreateErrorResponse("Analysis returned empty results.");
            }

            _logger.LogDebug("Parsing analysis response: {Content}", messageContent);

            var analysisResult = JsonSerializer.Deserialize<OpenAiAnalysisResult>(
                messageContent, _jsonOptions);

            if (analysisResult == null)
            {
                return CreateErrorResponse("Could not parse analysis results.");
            }

            return MapToJewelryAnalysisResponse(analysisResult);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response");
            return CreateErrorResponse("Analysis results could not be processed.");
        }
    }

    private JewelryAnalysisResponse MapToJewelryAnalysisResponse(OpenAiAnalysisResult result)
    {
        var response = new JewelryAnalysisResponse
        {
            Success = true,
            PieceDescription = result.PieceDescription ?? "A jewelry piece",
            ConfidenceNote = result.ConfidenceNote ?? "Analysis based on the provided image",
            AnalysisLimitations = result.AnalysisLimitations
        };

        // Map detected attributes
        if (result.DetectedAttributes != null)
        {
            response.DetectedAttributes = new DetectedJewelryAttributes
            {
                JewelryType = result.DetectedAttributes.JewelryType ?? "unknown",
                HasStones = result.DetectedAttributes.HasStones,
                StoneDescription = result.DetectedAttributes.StoneDescription,
                ApparentMetal = result.DetectedAttributes.ApparentMetal ?? "uncertain",
                ApparentFinish = result.DetectedAttributes.ApparentFinish ?? "polished",
                StyleCharacter = result.DetectedAttributes.StyleCharacter ?? "classic"
            };
        }

        // Map improvement categories
        if (result.ImprovementCategories != null)
        {
            response.ImprovementCategories = result.ImprovementCategories
                .Select(cat => new ImprovementCategory
                {
                    CategoryId = cat.CategoryId ?? string.Empty,
                    CategoryLabel = cat.CategoryLabel ?? string.Empty,
                    Suggestions = cat.Suggestions?.Select(s => new ImprovementSuggestion
                    {
                        SuggestionId = s.SuggestionId ?? Guid.NewGuid().ToString(),
                        Title = s.Title ?? string.Empty,
                        Description = s.Description ?? string.Empty,
                        Benefit = s.Benefit ?? string.Empty,
                        ImpactLevel = NormalizeImpactLevel(s.ImpactLevel),
                        CharacterNote = s.CharacterNote
                    }).ToList() ?? new List<ImprovementSuggestion>()
                })
                .Where(cat => cat.Suggestions.Count > 0)
                .ToList();
        }

        // Map keep original option
        if (result.KeepOriginal != null)
        {
            response.KeepOriginal = new KeepOriginalOption
            {
                Title = result.KeepOriginal.Title ?? "Keep Original Design",
                Description = result.KeepOriginal.Description ??
                    "Preserve the piece exactly as designed, honoring the original vision.",
                IsDefault = result.KeepOriginal.IsDefault
            };
        }

        // Map preview guidance
        if (result.PreviewGuidance != null)
        {
            response.PreviewGuidance = new PreviewGuidance
            {
                Summary = result.PreviewGuidance.Summary ?? string.Empty,
                KeyVisualChanges = result.PreviewGuidance.KeyVisualChanges ?? new List<string>()
            };
        }

        // Map clarification request
        if (result.ClarificationRequest != null)
        {
            response.ClarificationRequest = new ClarificationRequest
            {
                Type = result.ClarificationRequest.Type ?? "image_quality",
                Message = result.ClarificationRequest.Message ?? string.Empty
            };
        }

        _logger.LogInformation(
            "Analysis complete: {JewelryType}, {CategoryCount} categories, {HasStones} stones",
            response.DetectedAttributes.JewelryType,
            response.ImprovementCategories.Count,
            response.DetectedAttributes.HasStones);

        return response;
    }

    private static string NormalizeImpactLevel(string? level)
    {
        return level?.ToLowerInvariant() switch
        {
            "subtle" => "subtle",
            "moderate" => "moderate",
            "bold" => "bold",
            _ => "moderate"
        };
    }

    private static JewelryAnalysisResponse CreateErrorResponse(string message)
    {
        return new JewelryAnalysisResponse
        {
            Success = false,
            ErrorMessage = message,
            PieceDescription = "Analysis unavailable",
            ConfidenceNote = "Unable to complete analysis",
            KeepOriginal = new KeepOriginalOption()
        };
    }
}

#region OpenAI API DTOs

internal class OpenAiChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("response_format")]
    public OpenAiResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("messages")]
    public List<OpenAiMessage> Messages { get; set; } = new();
}

internal class OpenAiResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "json_object";
}

internal class OpenAiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public object Content { get; set; } = string.Empty;
}

internal class OpenAiTextContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

internal class OpenAiImageContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "image_url";

    [JsonPropertyName("image_url")]
    public OpenAiImageUrl ImageUrl { get; set; } = new();
}

internal class OpenAiImageUrl
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = "high";
}

internal class OpenAiChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAiChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; set; }
}

internal class OpenAiChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAiResponseMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal class OpenAiResponseMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

internal class OpenAiUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

#endregion

#region Analysis Response DTOs

internal class OpenAiAnalysisResult
{
    [JsonPropertyName("piece_description")]
    public string? PieceDescription { get; set; }

    [JsonPropertyName("confidence_note")]
    public string? ConfidenceNote { get; set; }

    [JsonPropertyName("detected_attributes")]
    public OpenAiDetectedAttributes? DetectedAttributes { get; set; }

    [JsonPropertyName("improvement_categories")]
    public List<OpenAiImprovementCategory>? ImprovementCategories { get; set; }

    [JsonPropertyName("keep_original")]
    public OpenAiKeepOriginal? KeepOriginal { get; set; }

    [JsonPropertyName("preview_guidance")]
    public OpenAiPreviewGuidance? PreviewGuidance { get; set; }

    [JsonPropertyName("analysis_limitations")]
    public string? AnalysisLimitations { get; set; }

    [JsonPropertyName("clarification_request")]
    public OpenAiClarificationRequest? ClarificationRequest { get; set; }
}

internal class OpenAiDetectedAttributes
{
    [JsonPropertyName("jewelry_type")]
    public string? JewelryType { get; set; }

    [JsonPropertyName("has_stones")]
    public bool HasStones { get; set; }

    [JsonPropertyName("stone_description")]
    public string? StoneDescription { get; set; }

    [JsonPropertyName("apparent_metal")]
    public string? ApparentMetal { get; set; }

    [JsonPropertyName("apparent_finish")]
    public string? ApparentFinish { get; set; }

    [JsonPropertyName("style_character")]
    public string? StyleCharacter { get; set; }
}

internal class OpenAiImprovementCategory
{
    [JsonPropertyName("category_id")]
    public string? CategoryId { get; set; }

    [JsonPropertyName("category_label")]
    public string? CategoryLabel { get; set; }

    [JsonPropertyName("suggestions")]
    public List<OpenAiSuggestion>? Suggestions { get; set; }
}

internal class OpenAiSuggestion
{
    [JsonPropertyName("suggestion_id")]
    public string? SuggestionId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("benefit")]
    public string? Benefit { get; set; }

    [JsonPropertyName("impact_level")]
    public string? ImpactLevel { get; set; }

    [JsonPropertyName("character_note")]
    public string? CharacterNote { get; set; }
}

internal class OpenAiKeepOriginal
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; } = true;
}

internal class OpenAiPreviewGuidance
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("key_visual_changes")]
    public List<string>? KeyVisualChanges { get; set; }
}

internal class OpenAiClarificationRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

#endregion
