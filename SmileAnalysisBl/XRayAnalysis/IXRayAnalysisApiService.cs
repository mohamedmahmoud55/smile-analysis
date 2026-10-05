using SmileAnalysisBl.XRayAnalysis.Models;

namespace SmileAnalysisBl.XRayAnalysis;

public interface IXRayAnalysisApiService
{
    Task<XrHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default);

    Task<XrPanoramicAnalysis> AnalyzePanoramicAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    Task<XrCvmResult> AnalyzeCvmAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    Task<XrCephLandmarkAnalysis> AnalyzeCephLandmarksAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    Task<XrCephalometricSummary> AnalyzeCephAsync(
        XrCephalometricAnalysisRequest request, CancellationToken cancellationToken = default);

    Task<XrFullCaseAnalysis> AnalyzeFullCaseAsync(
        XrFullCaseAnalysisRequest request, CancellationToken cancellationToken = default);
}
