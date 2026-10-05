using SmileAnalysisBl.ViewModels.GummySmileViewModels;
using SmileAnalysisBl.ViewModels.PatientViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IGummySmileCaseService
{
    Task<int> StartCaseAsync(int patientId, CancellationToken cancellationToken = default);
    GummySmileUploadViewModel? GetUploadPage(int localCaseId);
    Task<bool> UploadImagesAsync(int localCaseId, Stream? restStream, string? restFileName, string? restContentType,
        Stream? smileStream, string? smileFileName, string? smileContentType, CancellationToken cancellationToken = default);
    GummySmileAnalyzeViewModel? GetAnalyzePage(int localCaseId);
    Task<bool> RunAnalysisAsync(int localCaseId, CancellationToken cancellationToken = default);
    GummySmileReviewViewModel? GetReviewPage(int localCaseId);
    Task<bool> SubmitOverridesAsync(GummySmileReviewViewModel model, CancellationToken cancellationToken = default);
    GummySmileClinicalViewModel? GetClinicalPage(int localCaseId);
    Task<bool> SaveClinicalDraftAsync(GummySmileClinicalViewModel model);
    Task<bool> SubmitClinicalAsync(GummySmileClinicalViewModel model, CancellationToken cancellationToken = default);
    Task<GummySmileDiagnosisViewModel?> GetDiagnosisPageAsync(int localCaseId, CancellationToken cancellationToken = default);
    Task<GummySmileTreatmentViewModel?> GetTreatmentPageAsync(int localCaseId, CancellationToken cancellationToken = default);
    Task<(byte[] Data, string ContentType)?> GetAssetAsync(int localCaseId, string assetPath, CancellationToken cancellationToken = default);
    Task<(byte[] Data, string FileName)?> GetReportPdfAsync(int localCaseId, CancellationToken cancellationToken = default);
    IReadOnlyList<PreviousAnalysisViewModel> GetPatientAnalyses(int patientId);
}
