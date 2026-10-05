using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.DentalGemma;

public class DentalGemmaApiService : IDentalGemmaApiService
{
    private readonly HttpClient _httpClient;
    private readonly DentalGemmaApiOptions _options;
    private readonly ILogger<DentalGemmaApiService> _logger;

    public DentalGemmaApiService(
        HttpClient httpClient,
        IOptions<DentalGemmaApiOptions> options,
        ILogger<DentalGemmaApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
        if (_httpClient.BaseAddress is null)
            _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<DgHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("health", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DgHealthResponse>(DentalGemmaJson.Options, cancellationToken);
    }

    public async Task<string> CompleteChatAsync(
        string systemPrompt,
        string userPayload,
        CancellationToken cancellationToken = default)
    {
        var request = new DgChatCompletionRequest
        {
            Model = _options.ModelId,
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature,
            Messages =
            [
                new DgChatMessage { Role = "system", Content = systemPrompt },
                new DgChatMessage { Role = "user", Content = userPayload }
            ]
        };

        var json = JsonSerializer.Serialize(request, DentalGemmaJson.Options);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("v1/chat/completions", content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var completion = JsonSerializer.Deserialize<DgChatCompletionResponse>(body, DentalGemmaJson.Options);
        var text = completion?.Choices.FirstOrDefault()?.Message.Content;

        if (string.IsNullOrWhiteSpace(text))
            throw new DentalGemmaApiException(502, "DentalGemma returned an empty response.");

        return text;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("DentalGemma API error {StatusCode}: {Body}", (int)response.StatusCode, body);
        throw new DentalGemmaApiException((int)response.StatusCode, ExtractErrorMessage(body));
    }

    private static string ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "The DentalGemma service returned an error.";

        return body.Length > 500 ? body[..500] : body;
    }
}

public class DentalGemmaApiException : Exception
{
    public int StatusCode { get; }

    public DentalGemmaApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
