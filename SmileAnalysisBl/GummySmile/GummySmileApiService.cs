using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmileAnalysisBl.GummySmile.Models;

namespace SmileAnalysisBl.GummySmile;

public class GummySmileApiService : IGummySmileApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GummySmileApiService> _logger;

    public GummySmileApiService(HttpClient httpClient, IOptions<GummySmileApiOptions> options, ILogger<GummySmileApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = options.Value.BaseUrl.TrimEnd('/') + "/";
        if (_httpClient.BaseAddress is null)
            _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<GsHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/health", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadJsonAsync<GsHealthResponse>(response, cancellationToken);
    }

    public async Task<GsCaseDto> CreateCaseAsync(GsCreateCaseRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "api/cases",
            CreateJsonContent(request),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<GsCaseDto> GetCaseAsync(string gsCaseId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/cases/{gsCaseId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<GsCaseDto> UploadImageAsync(
        string gsCaseId,
        GsImageRole role,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        var roleValue = role == GsImageRole.Rest ? "rest" : "smile";
        using var response = await _httpClient.PostAsync(
            $"api/cases/{gsCaseId}/images?role={roleValue}",
            form,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<GsCaseDto> AnalyzeAsync(string gsCaseId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/cases/{gsCaseId}/analyze", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<GsCaseDto> UpdateClinicalAsync(
        string gsCaseId,
        GsClinicalUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            $"api/cases/{gsCaseId}/clinical",
            CreateJsonContent(request),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<GsCaseDto> UpdateOverridesAsync(
        string gsCaseId,
        GsOverridesRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsync(
            $"api/cases/{gsCaseId}/overrides",
            CreateJsonContent(request),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<GsCaseDto>(response, cancellationToken))!;
    }

    public async Task<byte[]> GetReportPdfAsync(string gsCaseId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/cases/{gsCaseId}/report.pdf", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<string> GetReportJsonAsync(string gsCaseId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/cases/{gsCaseId}/report.json", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<(byte[] Data, string ContentType)> GetAssetAsync(
        string gsCaseId, string assetPath, CancellationToken cancellationToken = default)
    {
        var encodedPath = string.Join("/", assetPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));
        using var response = await _httpClient.GetAsync($"api/cases/{gsCaseId}/assets/{encodedPath}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? GuessContentType(assetPath);
        return (data, contentType);
    }

    private static string GuessContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "application/octet-stream"
    };

    private static StringContent CreateJsonContent<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, GummySmileJson.Options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, GummySmileJson.Options);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("Gummy Smile API error {StatusCode}: {Body}", (int)response.StatusCode, body);
        throw new GummySmileApiException((int)response.StatusCode, ExtractErrorMessage(body));
    }

    private static string ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "The Gummy Smile AI service returned an error.";

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                if (detail.ValueKind == JsonValueKind.String)
                    return detail.GetString() ?? body;

                if (detail.ValueKind == JsonValueKind.Array && detail.GetArrayLength() > 0)
                {
                    var first = detail[0];
                    if (first.TryGetProperty("msg", out var msg))
                        return msg.GetString() ?? body;
                }
            }
        }
        catch (JsonException)
        {
            // fall through
        }

        return body.Length > 500 ? body[..500] : body;
    }
}

public class GummySmileApiException : Exception
{
    public int StatusCode { get; }

    public GummySmileApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
