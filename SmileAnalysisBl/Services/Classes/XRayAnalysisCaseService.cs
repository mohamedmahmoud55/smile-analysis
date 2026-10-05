using System.Globalization;
using SmileAnalysisBl.DentalGemma;
using SmileAnalysisBl.DentalGemma.Models;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PatientViewModels;
using SmileAnalysisBl.XRayAnalysis;
using SmileAnalysisBl.XRayAnalysis.Models;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class XRayAnalysisCaseService : IXRayAnalysisCaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IXRayAnalysisApiService _api;
    private readonly IDentalGemmaService _dentalGemma;

    public XRayAnalysisCaseService(
        IUnitOfWork unitOfWork,
        IXRayAnalysisApiService api,
        IDentalGemmaService dentalGemma)
    {
        _unitOfWork = unitOfWork;
        _api = api;
        _dentalGemma = dentalGemma;
    }

    public XRayAnalysisPageViewModel? GetAnalysisPage(int patientId, int? caseId = null)
    {
        var patient = _unitOfWork.GetRepository<Patient>().GetById(patientId);
        if (patient is null) return null;

        var model = new XRayAnalysisPageViewModel
        {
            PatientId = patientId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            DisplayId = $"P-{patientId:0000}",
            PatientAge = CalculateAge(patient.DateOfBirth),
            PatientSex = patient.Gender == Gender.Male ? "male" : "female"
        };

        if (caseId is null) return model;

        var localCase = GetCase(caseId.Value, patientId);
        if (localCase is null) return model;

        PopulateResults(model, localCase);
        return model;
    }

    public async Task<int> RunPanoramicAnalysisAsync(
        int patientId,
        int? caseId,
        Stream panoramicStream,
        string panoramicFileName,
        string panoramicContentType,
        CancellationToken cancellationToken = default)
    {
        var patient = GetPatient(patientId);
        var (localCase, results, isNew) = LoadOrCreateCase(patientId, caseId);

        var panoramicBytes = await ReadStreamAsync(panoramicStream, cancellationToken);
        using var ms = new MemoryStream(panoramicBytes);
        results.Panoramic = await _api.AnalyzePanoramicAsync(
            ms, panoramicFileName, panoramicContentType, cancellationToken);

        await TryRunDentalGemmaAsync(results, patient, runDentalGemma: false, cancellationToken);

        localCase.PanoramicImage = panoramicBytes;
        localCase.PanoramicImageContentType = panoramicContentType;
        FinalizeCase(localCase, results, isNew);
        return localCase.Id;
    }

    public async Task<int> RunCephalogramAnalysisAsync(
        int patientId,
        int? caseId,
        Stream cephStream,
        string cephFileName,
        string cephContentType,
        bool runDentalGemma,
        CancellationToken cancellationToken = default)
    {
        var patient = GetPatient(patientId);
        var (localCase, results, isNew) = LoadOrCreateCase(patientId, caseId);

        var cephBytes = await ReadStreamAsync(cephStream, cancellationToken);

        using (var landmarksMs = new MemoryStream(cephBytes))
        {
            results.CephLandmarks = await _api.AnalyzeCephLandmarksAsync(
                landmarksMs, cephFileName, cephContentType, cancellationToken);
        }

        var landmarks = results.CephLandmarks.Landmarks
            .GroupBy(l => l.Name)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var point = g.First().Point;
                    return new XrPoint2D { X = point.X, Y = point.Y, V = point.V };
                });

        using (var cvmMs = new MemoryStream(cephBytes))
        {
            results.Cvm = await _api.AnalyzeCvmAsync(
                cvmMs, cephFileName, cephContentType, cancellationToken);
        }

        results.CephSummary = await _api.AnalyzeCephAsync(new XrCephalometricAnalysisRequest
        {
            Landmarks = landmarks,
            CvmStage = results.Cvm.Stage,
            RoiBbox = results.Cvm.RoiBbox
        }, cancellationToken);

        await TryRunDentalGemmaAsync(results, patient, runDentalGemma, cancellationToken);

        localCase.CephImage = cephBytes;
        localCase.CephImageContentType = cephContentType;
        FinalizeCase(localCase, results, isNew);
        return localCase.Id;
    }

    public async Task<int> RunFullCaseSynthesisAsync(
        int patientId,
        int caseId,
        CancellationToken cancellationToken = default)
    {
        var localCase = GetCase(caseId, patientId)
            ?? throw new XRayAnalysisApiException(404, "Analysis case not found.");

        var results = XrAnalysisResults.Deserialize(localCase.ResultsJson) ?? new XrAnalysisResults();
        if (results.Panoramic is null || results.CephSummary is null || results.Cvm is null)
            throw new XRayAnalysisApiException(400, "Both panoramic and cephalogram results are required for integrated CDSS.");

        results.FullCase = await _api.AnalyzeFullCaseAsync(new XrFullCaseAnalysisRequest
        {
            PanoramicFindings = results.Panoramic,
            CephalometricSummary = results.CephSummary,
            CvmSummary = results.Cvm
        }, cancellationToken);

        FinalizeCase(localCase, results, isNew: false);
        return localCase.Id;
    }

    public IReadOnlyList<PreviousAnalysisViewModel> GetPatientAnalyses(int patientId)
    {
        return _unitOfWork.GetRepository<XRayAnalysisCase>()
            .GetAll()
            .Where(c => c.PatientId == patientId && c.CompletedAt != null)
            .OrderByDescending(c => c.CompletedAt)
            .Select(c => new PreviousAnalysisViewModel
            {
                Id = c.Id,
                AnalysisType = "X-Ray & CDSS",
                ThumbnailPlaceholder = "XR",
                ThumbnailClass = "pa-thumb-xray",
                AnalysisDate = c.CompletedAt!.Value,
                DateDisplay = c.CompletedAt!.Value.ToString("MMM dd, yyyy · h:mm tt", CultureInfo.InvariantCulture),
                Result = c.Summary ?? "Radiographic analysis complete",
                Status = "Completed",
                StatusBadgeClass = "pa-badge-success",
                ConfidencePercent = c.ConfidencePercent
            })
            .ToList();
    }

    private Patient GetPatient(int patientId) =>
        _unitOfWork.GetRepository<Patient>().GetById(patientId)
        ?? throw new InvalidOperationException("Patient not found.");

    private (XRayAnalysisCase localCase, XrAnalysisResults results, bool isNew) LoadOrCreateCase(int patientId, int? caseId)
    {
        if (caseId is not null)
        {
            var existing = GetCase(caseId.Value, patientId)
                ?? throw new XRayAnalysisApiException(404, "Analysis case not found.");
            var results = XrAnalysisResults.Deserialize(existing.ResultsJson) ?? new XrAnalysisResults();
            return (existing, results, false);
        }

        return (new XRayAnalysisCase
        {
            PatientId = patientId,
            CreatedAt = DateTime.UtcNow
        }, new XrAnalysisResults(), true);
    }

    private async Task TryRunDentalGemmaAsync(
        XrAnalysisResults results,
        Patient patient,
        bool runDentalGemma,
        CancellationToken cancellationToken)
    {
        if (!runDentalGemma || results.CephSummary is null || results.Cvm is null)
            return;

        var request = _dentalGemma.BuildOrthodonticRequestFromXRayResults(results);
        var invocation = await _dentalGemma.InvokeAsync(
            DentalGemmaMode.OrthodonticCaseExplanation,
            request,
            cancellationToken);

        var explanation = invocation.OrthodonticCaseExplanation;
        results.DentalReport = new XrDentalGemmaReportResponse
        {
            OrthodonticExplanation = explanation,
            Audit = invocation.Audit,
            ReportJson = new XrDentalGemmaReport
            {
                RadiographicSummary = explanation?.CaseSummary ?? string.Empty,
                PatientFriendlySummary = string.Join(
                    " ",
                    explanation?.OrthodonticOptionsExplanation ?? [])
            },
            TreatmentPlanLogic = results.FullCase?.TreatmentPlanLogic ?? new XrTreatmentPlanLogic()
        };
    }

    private void FinalizeCase(XRayAnalysisCase localCase, XrAnalysisResults results, bool isNew)
    {
        results.CompletedAt = DateTime.UtcNow;
        localCase.ResultsJson = results.Serialize();
        localCase.CompletedAt = results.CompletedAt;
        localCase.LandmarkCount = results.CephLandmarks?.Landmarks.Count;
        localCase.CvmStage = results.Cvm?.Stage;
        localCase.SkeletalClass = results.CephSummary?.SkeletalClass ?? results.FullCase?.CephalometricDiagnosis.SkeletalClass;
        localCase.ConfidencePercent = ComputeConfidence(results);
        localCase.Summary = BuildSummary(results);

        var repo = _unitOfWork.GetRepository<XRayAnalysisCase>();
        if (isNew)
            repo.Add(localCase);
        else
            repo.Update(localCase);

        _unitOfWork.SaveChanges();
    }

    private void PopulateResults(XRayAnalysisPageViewModel model, XRayAnalysisCase localCase)
    {
        model.CaseId = localCase.Id;
        model.Results = XrAnalysisResults.Deserialize(localCase.ResultsJson);
        model.CephSummary = XRayDisplayHelpers.ResolveCephSummary(model.Results);
        model.CephMeasurementRows = XRayDisplayHelpers.BuildCephMeasurementRows(model.CephSummary);
        model.PanoramicOverlayDataUrl = ToDataUrl(localCase.PanoramicImageContentType, model.Results?.Panoramic?.OverlayImageBase64);
        model.CephOverlayDataUrl = ToDataUrl("image/png", model.CephSummary?.OverlayImageBase64
            ?? model.Results?.CephLandmarks?.OverlayImageBase64);
        model.HasPanoramicResults = model.Results?.Panoramic is not null;
        model.HasCephalogramResults = model.Results?.CephLandmarks is not null;
        model.CanSynthesizeFullCase = model.HasPanoramicResults
            && model.HasCephalogramResults
            && model.Results?.FullCase is null;
    }

    private XRayAnalysisCase? GetCase(int caseId, int patientId)
    {
        var localCase = _unitOfWork.GetRepository<XRayAnalysisCase>().GetById(caseId);
        return localCase?.PatientId == patientId ? localCase : null;
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }

    private static int ComputeConfidence(XrAnalysisResults results)
    {
        var scores = new List<double>();
        if (results.CephLandmarks?.Landmarks.Count > 0)
            scores.Add(results.CephLandmarks.Landmarks.Average(l => l.Confidence) * 100);
        if (results.Cvm?.Confidence > 0)
            scores.Add(results.Cvm.Confidence * 100);
        if (results.Panoramic?.Detections.Count > 0)
            scores.Add(results.Panoramic.Detections.Average(d => d.Confidence) * 100);
        return scores.Count > 0 ? (int)Math.Round(scores.Average()) : 0;
    }

    private static string BuildSummary(XrAnalysisResults results)
    {
        var parts = new List<string>();
        if (results.CephLandmarks?.Landmarks.Count is { } count and > 0)
            parts.Add($"{count} landmarks detected");
        if (!string.IsNullOrEmpty(results.Cvm?.Stage))
            parts.Add($"CVM {results.Cvm.Stage}");
        if (!string.IsNullOrEmpty(results.CephSummary?.SkeletalClass) && results.CephSummary.SkeletalClass != "undetermined")
            parts.Add($"Skeletal Class {results.CephSummary.SkeletalClass}");
        if (results.Panoramic?.BlockedUntilDentalClearance == true)
            parts.Add("dental clearance required");
        if (!string.IsNullOrEmpty(results.FullCase?.TreatmentPlanLogic.MainRecommendation))
            parts.Add(results.FullCase.TreatmentPlanLogic.MainRecommendation);
        return parts.Count > 0 ? string.Join(" · ", parts) : "Radiographic analysis complete";
    }

    private static string? ToDataUrl(string? contentType, string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;
        var mime = contentType ?? "image/png";
        return $"data:{mime};base64,{base64}";
    }
}
