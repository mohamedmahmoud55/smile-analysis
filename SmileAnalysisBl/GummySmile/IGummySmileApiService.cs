using SmileAnalysisBl.GummySmile.Models;

namespace SmileAnalysisBl.GummySmile;

public interface IGummySmileApiService
{
    Task<GsHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default);
    Task<GsCaseDto> CreateCaseAsync(GsCreateCaseRequest request, CancellationToken cancellationToken = default);
    Task<GsCaseDto> GetCaseAsync(string gsCaseId, CancellationToken cancellationToken = default);
    Task<GsCaseDto> UploadImageAsync(
        string gsCaseId,
        GsImageRole role,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
    Task<GsCaseDto> AnalyzeAsync(string gsCaseId, CancellationToken cancellationToken = default);
    Task<GsCaseDto> UpdateClinicalAsync(
        string gsCaseId,
        GsClinicalUpdateRequest request,
        CancellationToken cancellationToken = default);
    Task<GsCaseDto> UpdateOverridesAsync(
        string gsCaseId,
        GsOverridesRequest request,
        CancellationToken cancellationToken = default);
    Task<byte[]> GetReportPdfAsync(string gsCaseId, CancellationToken cancellationToken = default);
    Task<string> GetReportJsonAsync(string gsCaseId, CancellationToken cancellationToken = default);
    Task<(byte[] Data, string ContentType)> GetAssetAsync(string gsCaseId, string assetPath, CancellationToken cancellationToken = default);
}
