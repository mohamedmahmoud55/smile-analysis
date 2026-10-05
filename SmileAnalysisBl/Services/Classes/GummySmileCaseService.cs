using System.Globalization;
using System.Text.Json;
using SmileAnalysisBl.GummySmile;
using SmileAnalysisBl.GummySmile.Models;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.GummySmileViewModels;
using SmileAnalysisBl.ViewModels.PatientViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class GummySmileCaseService : IGummySmileCaseService
{
    private const double LowScaleConfidenceThreshold = 0.45;

    private static readonly JsonSerializerOptions DraftJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IGummySmileApiService _api;

    public GummySmileCaseService(IUnitOfWork unitOfWork, IGummySmileApiService api)
    {
        _unitOfWork = unitOfWork;
        _api = api;
    }

    public async Task<int> StartCaseAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = _unitOfWork.GetRepository<Patient>().GetById(patientId)
            ?? throw new InvalidOperationException("Patient not found.");

        var gsCase = await _api.CreateCaseAsync(new GsCreateCaseRequest
        {
            Patient = MapPatientInfo(patient)
        }, cancellationToken);

        var localCase = new GummySmileCase
        {
            PatientId = patientId,
            GsCaseId = gsCase.Id,
            CreatedAt = DateTime.UtcNow
        };
        GummySmileCaseStore.ApplySnapshotFields(localCase, gsCase);

        _unitOfWork.GetRepository<GummySmileCase>().Add(localCase);
        _unitOfWork.SaveChanges();
        return localCase.Id;
    }

    public GummySmileUploadViewModel? GetUploadPage(int localCaseId)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return null;

        var gsCase = ResolveGsCase(localCase);
        var images = gsCase?.Status >= GsCaseStatus.ImagesUploaded;

        var model = MapBase<GummySmileUploadViewModel>(localCase, GummySmileWizardStep.Intake);
        model.RestImageUploaded = images;
        model.SmileImageUploaded = images;
        model.AnalyzeCompleted = gsCase?.Images?.Values.Any(i => !string.IsNullOrEmpty(i.Crop?.CropPath)) == true;
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        return model;
    }

    public async Task<bool> UploadImagesAsync(
        int localCaseId,
        Stream? restStream, string? restFileName, string? restContentType,
        Stream? smileStream, string? smileFileName, string? smileContentType,
        CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return false;
        if (restStream is null || smileStream is null) return false;

        var restBytes = await ReadStreamAsync(restStream, cancellationToken);
        var smileBytes = await ReadStreamAsync(smileStream, cancellationToken);

        localCase.RestImage = restBytes;
        localCase.RestImageContentType = restContentType ?? "image/jpeg";
        localCase.SmileImage = smileBytes;
        localCase.SmileImageContentType = smileContentType ?? "image/jpeg";

        await _api.UploadImageAsync(
            localCase.GsCaseId, GsImageRole.Rest, new MemoryStream(restBytes),
            restFileName ?? "rest.jpg", localCase.RestImageContentType, cancellationToken);

        await _api.UploadImageAsync(
            localCase.GsCaseId, GsImageRole.Smile, new MemoryStream(smileBytes),
            smileFileName ?? "smile.jpg", localCase.SmileImageContentType, cancellationToken);

        var gsCase = await _api.GetCaseAsync(localCase.GsCaseId, cancellationToken);
        await PersistSnapshotAsync(localCase, gsCase, cacheAssets: false, cancellationToken);
        return true;
    }

    public GummySmileAnalyzeViewModel? GetAnalyzePage(int localCaseId)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return null;

        var gsCase = ResolveGsCase(localCase);
        var uploaded = gsCase?.Status >= GsCaseStatus.ImagesUploaded;

        var model = MapBase<GummySmileAnalyzeViewModel>(localCase, GummySmileWizardStep.Intake);
        model.RestImageUploaded = uploaded;
        model.SmileImageUploaded = uploaded;
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        return model;
    }

    public async Task<bool> RunAnalysisAsync(int localCaseId, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return false;

        var gsCase = await _api.AnalyzeAsync(localCase.GsCaseId, cancellationToken);
        await PersistSnapshotAsync(localCase, gsCase, cacheAssets: true, cancellationToken);
        return true;
    }

    public GummySmileReviewViewModel? GetReviewPage(int localCaseId)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null || !IsAnalyzed(localCase)) return null;

        var gsCase = ResolveGsCase(localCase);
        if (gsCase is null) return null;

        var warnings = new List<string>();
        var lowScale = false;
        if (gsCase.Images is not null)
        {
            foreach (var image in gsCase.Images.Values)
            {
                warnings.AddRange(image.Warnings);
                if (image.Scale.Confidence < LowScaleConfidenceThreshold)
                    lowScale = true;
            }
        }

        warnings.AddRange(gsCase.Measurements?.Warnings ?? []);

        var model = MapBase<GummySmileReviewViewModel>(localCase, GummySmileWizardStep.Review);
        model.Case = gsCase;
        model.Measurements = gsCase.Measurements;
        model.AiWarnings = warnings.Distinct().ToList();
        model.LowScaleConfidence = lowScale;
        model.GdOverrides = BuildGdOverrideRows(gsCase.Measurements?.TeethGd ?? []);
        model.RestOverlayPath = gsCase.Images?.GetValueOrDefault("rest")?.OverlayPath
            ?? gsCase.Images?.Values.FirstOrDefault(i => i.Role == GsImageRole.Rest)?.OverlayPath;
        model.SmileOverlayPath = GummySmileDisplayHelpers.SmileOverlayPath(gsCase);
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        return model;
    }

    public async Task<bool> SubmitOverridesAsync(GummySmileReviewViewModel model, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(model.LocalCaseId);
        if (localCase is null || !IsAnalyzed(localCase)) return false;

        var overrides = model.GdOverrides
            .Where(r => r.OverrideGdMm.HasValue)
            .Select(r => new GsToothGdOverride
            {
                Fdi = r.Fdi,
                SegClass = r.SegClass,
                Side = r.Side,
                GdMm = r.OverrideGdMm!.Value
            })
            .ToList();

        if (overrides.Count == 0)
            return true;

        var request = new GsOverridesRequest
        {
            TeethGd = overrides,
            Recompute = true
        };

        localCase.OverridesRequestJson = JsonSerializer.Serialize(request, GummySmileJson.Options);
        var gsCase = await _api.UpdateOverridesAsync(localCase.GsCaseId, request, cancellationToken);
        await PersistSnapshotAsync(localCase, gsCase, cacheAssets: true, cancellationToken);
        return true;
    }

    public GummySmileClinicalViewModel? GetClinicalPage(int localCaseId)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null || !IsAnalyzed(localCase)) return null;

        if (!string.IsNullOrEmpty(localCase.ClinicalDraftJson))
        {
            var draft = JsonSerializer.Deserialize<GummySmileClinicalViewModel>(localCase.ClinicalDraftJson, DraftJsonOptions);
            if (draft is not null)
            {
                PopulateBase(draft, localCase, GummySmileWizardStep.Clinical);
                draft.HasDraft = true;
                return draft;
            }
        }

        var gsCase = ResolveGsCase(localCase);
        var clinical = gsCase?.Clinical;

        var model = MapBase<GummySmileClinicalViewModel>(localCase, GummySmileWizardStep.Clinical);
        model.U1FeopMm = clinical?.U1FeopMm;
        model.SnU1Deg = clinical?.SnU1Deg;
        model.NfcADeg = clinical?.NfcADeg;
        model.FacialAxisDeg = clinical?.FacialAxisDeg;
        model.CrownWidthMm = clinical?.CrownWidthMm;
        model.CrownLengthMm = clinical?.CrownLengthMm;
        model.ProbingDepths = BuildProbingInputs(clinical?.ProbingDepthsMm);
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        return model;
    }

    public Task<bool> SaveClinicalDraftAsync(GummySmileClinicalViewModel model)
    {
        var localCase = GetLocalCase(model.LocalCaseId);
        if (localCase is null || !IsAnalyzed(localCase))
            return Task.FromResult(false);

        model.HasDraft = true;
        localCase.ClinicalDraftJson = JsonSerializer.Serialize(model, DraftJsonOptions);
        localCase.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        return Task.FromResult(_unitOfWork.SaveChanges() > 0);
    }

    public async Task<bool> SubmitClinicalAsync(GummySmileClinicalViewModel model, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(model.LocalCaseId);
        if (localCase is null || !IsAnalyzed(localCase)) return false;

        var request = new GsClinicalUpdateRequest
        {
            Clinical = BuildClinicalInputs(model),
            Patient = MapPatientInfo(localCase.Patient)
        };

        localCase.ClinicalRequestJson = JsonSerializer.Serialize(request, GummySmileJson.Options);
        localCase.ClinicalDraftJson = null;

        var gsCase = await _api.UpdateClinicalAsync(localCase.GsCaseId, request, cancellationToken);
        await PersistSnapshotAsync(localCase, gsCase, cacheAssets: true, cancellationToken);

        if (gsCase.Diagnosis is null)
            return false;

        await PersistReportAsync(localCase, gsCase, cancellationToken);
        return true;
    }

    public async Task<GummySmileDiagnosisViewModel?> GetDiagnosisPageAsync(int localCaseId, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return null;

        var gsCase = await ResolveGsCaseAsync(localCase, cancellationToken);
        if (gsCase is null) return null;

        var model = MapBase<GummySmileDiagnosisViewModel>(localCase, GummySmileWizardStep.Diagnosis);
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        model.Case = gsCase;
        model.CompletedAt = localCase.CompletedAt;
        model.IsReady = gsCase.Measurements is not null && gsCase.Diagnosis is not null;

        if (model.IsReady)
        {
            model.ToothChartJson = GummySmileDisplayHelpers.BuildToothChartJson(gsCase.Diagnosis);
            model.MeasurementRows = GummySmileDisplayHelpers.BuildMeasurementTable(gsCase);
            var (path, isOriginal) = await ResolveSmileDisplayPathAsync(localCase, gsCase, cancellationToken);
            model.SmileOverlayPath = path;
            model.SmileImageIsOriginal = isOriginal;
        }

        return model;
    }

    public async Task<GummySmileTreatmentViewModel?> GetTreatmentPageAsync(int localCaseId, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return null;

        var gsCase = await ResolveGsCaseAsync(localCase, cancellationToken);
        if (gsCase is null) return null;

        var model = MapBase<GummySmileTreatmentViewModel>(localCase, GummySmileWizardStep.Treatment);
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(gsCase);
        model.Case = gsCase;
        model.CompletedAt = localCase.CompletedAt;
        model.HasPdf = localCase.ReportPdf is { Length: > 0 };
        model.IsReady = gsCase.Treatment?.Items.Count > 0;

        if (model.IsReady && string.IsNullOrEmpty(localCase.ReportJson))
            await PersistReportAsync(localCase, gsCase, cancellationToken);

        return model;
    }

    public async Task<(byte[] Data, string ContentType)?> GetAssetAsync(
        int localCaseId, string assetPath, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null || string.IsNullOrWhiteSpace(assetPath)) return null;

        var normalized = assetPath.TrimStart('/');

        if (GummySmileLocalAssets.IsUploadedPath(normalized))
        {
            var role = normalized == GummySmileLocalAssets.Smile ? "smile" : "rest";
            return GummySmileCaseStore.TryGetUploadedImage(localCase, role);
        }

        var cached = GummySmileCaseStore.TryGetCachedAsset(localCase, normalized);
        if (cached is not null)
            return cached.Value;

        try
        {
            var result = await _api.GetAssetAsync(localCase.GsCaseId, normalized, cancellationToken);
            await CacheAssetAsync(localCase, normalized, result.Data, result.ContentType, cancellationToken);
            return result;
        }
        catch (GummySmileApiException)
        {
            if (GummySmileLocalAssets.IsSmileRelated(normalized))
                return GummySmileCaseStore.TryGetUploadedImage(localCase, "smile");
            if (GummySmileLocalAssets.IsRestRelated(normalized))
                return GummySmileCaseStore.TryGetUploadedImage(localCase, "rest");
            return null;
        }
    }

    public async Task<(byte[] Data, string FileName)?> GetReportPdfAsync(int localCaseId, CancellationToken cancellationToken = default)
    {
        var localCase = GetLocalCase(localCaseId);
        if (localCase is null) return null;

        if (localCase.ReportPdf is { Length: > 0 })
            return (localCase.ReportPdf, PdfFileName(localCase));

        var pdf = await _api.GetReportPdfAsync(localCase.GsCaseId, cancellationToken);
        localCase.ReportPdf = pdf;
        localCase.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        _unitOfWork.SaveChanges();

        return (pdf, PdfFileName(localCase));
    }

    public IReadOnlyList<PreviousAnalysisViewModel> GetPatientAnalyses(int patientId)
    {
        return _unitOfWork.GetRepository<GummySmileCase>()
            .GetAll()
            .Where(c => c.PatientId == patientId && c.CompletedAt != null)
            .OrderByDescending(c => c.CompletedAt)
            .Select(c => new PreviousAnalysisViewModel
            {
                Id = c.Id,
                AnalysisType = "Gummy Smile",
                ThumbnailPlaceholder = "GS",
                ThumbnailClass = "pa-thumb-gummy",
                AnalysisDate = c.CompletedAt ?? c.CreatedAt,
                DateDisplay = (c.CompletedAt ?? c.CreatedAt).ToString("MMM dd, yyyy · h:mm tt", CultureInfo.InvariantCulture),
                Result = c.DiagnosisSummary ?? "Analysis completed",
                Status = "Completed",
                StatusBadgeClass = "pa-badge-success",
                ConfidencePercent = null
            })
            .ToList();
    }

    private async Task PersistReportAsync(GummySmileCase localCase, GsCaseDto gsCase, CancellationToken cancellationToken)
    {
        if (gsCase.Diagnosis is null)
            throw new GummySmileApiException(422, "Diagnosis is not ready yet. Complete clinical inputs first.");

        var reportJson = await _api.GetReportJsonAsync(localCase.GsCaseId, cancellationToken);

        localCase.ReportJson = reportJson;
        GummySmileCaseStore.ApplySnapshotFields(localCase, gsCase);
        localCase.CompletedAt = DateTime.UtcNow;
        localCase.UpdatedAt = DateTime.UtcNow;

        try
        {
            localCase.ReportPdf = await _api.GetReportPdfAsync(localCase.GsCaseId, cancellationToken);
        }
        catch (GummySmileApiException)
        {
            // PDF may not be ready yet; results page can retry download
        }

        await CacheAssetsAsync(localCase, gsCase, cancellationToken);
        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        _unitOfWork.SaveChanges();
    }

    private async Task PersistSnapshotAsync(
        GummySmileCase localCase,
        GsCaseDto gsCase,
        bool cacheAssets,
        CancellationToken cancellationToken)
    {
        GummySmileCaseStore.ApplySnapshotFields(localCase, gsCase);
        localCase.UpdatedAt = DateTime.UtcNow;

        if (cacheAssets)
            await CacheAssetsAsync(localCase, gsCase, cancellationToken);

        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        _unitOfWork.SaveChanges();
    }

    private async Task CacheAssetsAsync(
        GummySmileCase localCase,
        GsCaseDto gsCase,
        CancellationToken cancellationToken)
    {
        var assets = GummySmileCaseStore.DeserializeAssets(localCase);

        foreach (var assetPath in GummySmileCaseStore.AssetPaths(gsCase).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (assets.ContainsKey(assetPath.TrimStart('/')))
                continue;

            try
            {
                var result = await _api.GetAssetAsync(localCase.GsCaseId, assetPath, cancellationToken);
                GummySmileCaseStore.AddCachedAsset(assets, assetPath, result.Data, result.ContentType);
            }
            catch (GummySmileApiException)
            {
                // Overlay may not be ready yet; asset endpoint can retry later
            }
        }

        if (assets.Count > 0)
            localCase.CachedAssetsJson = GummySmileCaseStore.SerializeAssets(assets);
    }

    private Task CacheAssetAsync(
        GummySmileCase localCase,
        string assetPath,
        byte[] data,
        string contentType,
        CancellationToken cancellationToken)
    {
        var assets = GummySmileCaseStore.DeserializeAssets(localCase);
        GummySmileCaseStore.AddCachedAsset(assets, assetPath, data, contentType);
        localCase.CachedAssetsJson = GummySmileCaseStore.SerializeAssets(assets);
        localCase.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        _unitOfWork.SaveChanges();
        return Task.CompletedTask;
    }

    private GsCaseDto? ResolveGsCase(GummySmileCase localCase)
    {
        var stored = TryResolveStoredCase(localCase);
        if (stored is not null)
            return stored;

        try
        {
            var gsCase = _api.GetCaseAsync(localCase.GsCaseId).GetAwaiter().GetResult();
            PersistSnapshotAsync(localCase, gsCase, cacheAssets: true, CancellationToken.None).GetAwaiter().GetResult();
            return gsCase;
        }
        catch (GummySmileApiException)
        {
            return null;
        }
    }

    private async Task<GsCaseDto?> ResolveGsCaseAsync(GummySmileCase localCase, CancellationToken cancellationToken)
    {
        var stored = GummySmileCaseStore.TryDeserializeStoredCase(localCase);
        if (stored is not null)
        {
            if (string.IsNullOrWhiteSpace(localCase.CaseSnapshotJson))
                BackfillSnapshot(localCase, stored);

            if (string.IsNullOrWhiteSpace(localCase.CachedAssetsJson))
                await CacheAssetsAsync(localCase, stored, cancellationToken);

            return stored;
        }

        try
        {
            var gsCase = await _api.GetCaseAsync(localCase.GsCaseId, cancellationToken);
            await PersistSnapshotAsync(localCase, gsCase, cacheAssets: true, cancellationToken);
            return gsCase;
        }
        catch (GummySmileApiException)
        {
            return null;
        }
    }

    private async Task<(string? Path, bool IsOriginal)> ResolveSmileDisplayPathAsync(
        GummySmileCase localCase,
        GsCaseDto gsCase,
        CancellationToken cancellationToken)
    {
        var overlay = GummySmileDisplayHelpers.SmileOverlayPath(gsCase);

        if (!string.IsNullOrEmpty(overlay))
        {
            if (GummySmileCaseStore.TryGetCachedAsset(localCase, overlay) is not null)
                return (overlay, false);

            try
            {
                var result = await _api.GetAssetAsync(localCase.GsCaseId, overlay.TrimStart('/'), cancellationToken);
                await CacheAssetAsync(localCase, overlay, result.Data, result.ContentType, cancellationToken);
                return (overlay, false);
            }
            catch (GummySmileApiException)
            {
                // Fall through to uploaded image or no image.
            }
        }

        if (localCase.SmileImage is { Length: > 0 })
            return (GummySmileLocalAssets.Smile, true);

        return (null, false);
    }

    private GsCaseDto? TryResolveStoredCase(GummySmileCase localCase)
    {
        var stored = GummySmileCaseStore.TryDeserializeStoredCase(localCase);
        if (stored is null)
            return null;

        if (string.IsNullOrWhiteSpace(localCase.CaseSnapshotJson))
            BackfillSnapshot(localCase, stored);

        if (string.IsNullOrWhiteSpace(localCase.CachedAssetsJson))
        {
            try
            {
                CacheAssetsAsync(localCase, stored, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (GummySmileApiException)
            {
                // API may no longer host assets for archived cases.
            }
        }

        return stored;
    }

    private void BackfillSnapshot(GummySmileCase localCase, GsCaseDto gsCase)
    {
        GummySmileCaseStore.ApplySnapshotFields(localCase, gsCase);
        localCase.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.GetRepository<GummySmileCase>().Update(localCase);
        _unitOfWork.SaveChanges();
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    private GummySmileCase? GetLocalCase(int localCaseId)
    {
        var localCase = _unitOfWork.GetRepository<GummySmileCase>()
            .GetAll()
            .FirstOrDefault(c => c.Id == localCaseId);

        if (localCase is null)
            return null;

        localCase.Patient = _unitOfWork.GetRepository<Patient>().GetById(localCase.PatientId)!;
        return localCase;
    }

    private static bool IsAnalyzed(GummySmileCase localCase) =>
        localCase.GsStatus is "analyzed" or "clinical_entered" or "diagnosed" or "planned";

    private static bool IsClinicalSubmitted(GsCaseDto gsCase) =>
        gsCase.Status >= GsCaseStatus.ClinicalEntered;

    private static GsClinicalInputs BuildClinicalInputs(GummySmileClinicalViewModel model)
    {
        var probing = model.ProbingDepths
            .Where(p => p.DepthMm.HasValue)
            .ToDictionary(p => p.Fdi, p => p.DepthMm!.Value);

        return new GsClinicalInputs
        {
            ProbingDepthsMm = probing.Count > 0 ? probing : null,
            U1FeopMm = model.U1FeopMm,
            SnU1Deg = model.SnU1Deg,
            NfcADeg = model.NfcADeg,
            FacialAxisDeg = model.FacialAxisDeg,
            CrownWidthMm = model.CrownWidthMm,
            CrownLengthMm = model.CrownLengthMm
        };
    }

    private static List<GummySmileProbingInput> BuildProbingInputs(Dictionary<string, double>? existing)
    {
        return GummySmileWizardFdi.UpperFdi.Select(fdi => new GummySmileProbingInput
        {
            Fdi = fdi,
            ToothLabel = $"Tooth {fdi}",
            DepthMm = existing is not null && existing.TryGetValue(fdi, out var depth) ? depth : null
        }).ToList();
    }

    private static List<GummySmileGdOverrideRow> BuildGdOverrideRows(IEnumerable<GsToothGd> teeth)
    {
        return teeth.Select(t => new GummySmileGdOverrideRow
        {
            Fdi = t.Fdi,
            SegClass = t.SegClass,
            Side = t.Side,
            AiGdMm = t.GdMm
        }).ToList();
    }

    private T MapBase<T>(GummySmileCase localCase, GummySmileWizardStep step) where T : GummySmileWizardBaseViewModel, new()
    {
        var model = new T();
        PopulateBase(model, localCase, step);
        return model;
    }

    private void PopulateBase(GummySmileWizardBaseViewModel model, GummySmileCase localCase, GummySmileWizardStep step)
    {
        model.LocalCaseId = localCase.Id;
        model.PatientId = localCase.PatientId;
        model.PatientName = PatientName(localCase.Patient);
        model.DisplayId = Mrn(localCase.PatientId);
        model.GsCaseId = localCase.GsCaseId;
        model.CurrentStep = step;
        model.GsStatus = localCase.GsStatus;
        model.PatientAge = CalculateAge(localCase.Patient.DateOfBirth);
        model.PatientSex = localCase.Patient.Gender == Gender.Male ? "male" : "female";
        model.StepStatus = GummySmileDisplayHelpers.ComputeStepStatus(ResolveGsCase(localCase));
    }

    private static GsPatientInfo MapPatientInfo(Patient patient) => new()
    {
        Name = $"{patient.FirstName} {patient.LastName}".Trim(),
        RecordId = Mrn(patient.Id),
        Age = CalculateAge(patient.DateOfBirth),
        Sex = patient.Gender == Gender.Male ? GsSex.Male : GsSex.Female
    };

    private static string Mrn(int patientId) => $"P-{patientId:0000}";

    private static string PatientName(Patient patient) => $"{patient.FirstName} {patient.LastName}".Trim();

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }

    private static string PdfFileName(GummySmileCase localCase) =>
        $"gummy-smile-report-{Mrn(localCase.PatientId)}-{localCase.Id}.pdf";
}
