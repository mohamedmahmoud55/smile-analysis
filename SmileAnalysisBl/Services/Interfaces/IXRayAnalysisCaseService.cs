using SmileAnalysisBl.ViewModels.PatientViewModels;
using SmileAnalysisBl.XRayAnalysis;
using SmileAnalysisBl.XRayAnalysis.Models;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IXRayAnalysisCaseService
{
    XRayAnalysisPageViewModel? GetAnalysisPage(int patientId, int? caseId = null);
    Task<int> RunPanoramicAnalysisAsync(
        int patientId,
        int? caseId,
        Stream panoramicStream, string panoramicFileName, string panoramicContentType,
        CancellationToken cancellationToken = default);

    Task<int> RunCephalogramAnalysisAsync(
        int patientId,
        int? caseId,
        Stream cephStream, string cephFileName, string cephContentType,
        bool runDentalGemma,
        CancellationToken cancellationToken = default);

    Task<int> RunFullCaseSynthesisAsync(
        int patientId,
        int caseId,
        CancellationToken cancellationToken = default);
    IReadOnlyList<PreviousAnalysisViewModel> GetPatientAnalyses(int patientId);
}

public class XRayAnalysisPageViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public int PatientAge { get; set; }
    public string PatientSex { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int? CaseId { get; set; }
    public XrAnalysisResults? Results { get; set; }
    public XrCephalometricSummary? CephSummary { get; set; }
    public IReadOnlyList<XrMeasurementTableRow> CephMeasurementRows { get; set; } = [];
    public string? PanoramicOverlayDataUrl { get; set; }
    public string? CephOverlayDataUrl { get; set; }
    public bool RunDentalGemma { get; set; }
    public bool HasPanoramicResults { get; set; }
    public bool HasCephalogramResults { get; set; }
    public bool CanSynthesizeFullCase { get; set; }
}
