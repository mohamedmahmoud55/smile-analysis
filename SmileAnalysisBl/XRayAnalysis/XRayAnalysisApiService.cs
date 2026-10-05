using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmileAnalysisBl.XRayAnalysis.Models;

namespace SmileAnalysisBl.XRayAnalysis;

public class XRayAnalysisApiService : IXRayAnalysisApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XRayAnalysisApiService> _logger;

    public XRayAnalysisApiService(
        HttpClient httpClient,
        IOptions<XRayAnalysisApiOptions> options,
        ILogger<XRayAnalysisApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = options.Value.BaseUrl.TrimEnd('/') + "/";
        if (_httpClient.BaseAddress is null)
            _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<XrHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/health", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadJsonAsync<XrHealthResponse>(response, cancellationToken);
    }

    public async Task<XrPanoramicAnalysis> AnalyzePanoramicAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var form = CreateFileForm(fileStream, fileName, contentType);
        using var response = await _httpClient.PostAsync("api/analyze/panoramic", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<XrPanoramicAnalysis>(response, cancellationToken))!;
    }

    public async Task<XrCvmResult> AnalyzeCvmAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var form = CreateFileForm(fileStream, fileName, contentType);
        using var response = await _httpClient.PostAsync("api/analyze/cvm", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<XrCvmResult>(response, cancellationToken))!;
    }

    public async Task<XrCephLandmarkAnalysis> AnalyzeCephLandmarksAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var form = CreateFileForm(fileStream, fileName, contentType);
        using var response = await _httpClient.PostAsync("api/analyze/ceph-landmarks", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<XrCephLandmarkAnalysis>(response, cancellationToken))!;
    }

    public async Task<XrCephalometricSummary> AnalyzeCephAsync(
        XrCephalometricAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "api/analyze/ceph",
            CreateJsonContent(request),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<XrCephalometricSummary>(response, cancellationToken))!;
    }

    public async Task<XrFullCaseAnalysis> AnalyzeFullCaseAsync(
        XrFullCaseAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "api/analyze/full-case",
            CreateJsonContent(request),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await ReadJsonAsync<XrFullCaseAnalysis>(response, cancellationToken))!;
    }

    private static MultipartFormDataContent CreateFileForm(Stream fileStream, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);
        return form;
    }

    private static StringContent CreateJsonContent<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, XRayAnalysisJson.Options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, XRayAnalysisJson.Options);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("X-Ray Analysis API error {StatusCode}: {Body}", (int)response.StatusCode, body);
        throw new XRayAnalysisApiException((int)response.StatusCode, ExtractErrorMessage(body));
    }

    private static string ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "The X-Ray Analysis service returned an error.";

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

public class XRayAnalysisApiException : Exception
{
    public int StatusCode { get; }

    public XRayAnalysisApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
