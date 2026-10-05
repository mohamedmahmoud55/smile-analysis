using System.Text.Json;
using Microsoft.Extensions.Options;
using SmileAnalysisBl.DentalGemma.Models;
using SmileAnalysisBl.XRayAnalysis.Models;

namespace SmileAnalysisBl.DentalGemma;

public interface IDentalGemmaService
{
    Task<DgInvocationResult> InvokeAsync(
        DentalGemmaMode mode,
        object requestPayload,
        CancellationToken cancellationToken = default);

    DgOrthodonticCaseExplanationRequest BuildOrthodonticRequestFromXRayResults(XrAnalysisResults results);

    DgStructuredReportRequest BuildStructuredReportRequest(
        XrAnalysisResults results,
        DgPatientContext patientContext);

    DgImageQaRequest BuildImageQaRequest(
        XrAnalysisResults results,
        string question,
        string imageType = "panoramic_xray");
}

public class DentalGemmaService : IDentalGemmaService
{
    private readonly IDentalGemmaApiService _api;
    private readonly DentalGemmaApiOptions _options;

    public DentalGemmaService(IDentalGemmaApiService api, IOptions<DentalGemmaApiOptions> options)
    {
        _api = api;
        _options = options.Value;
    }

    public async Task<DgInvocationResult> InvokeAsync(
        DentalGemmaMode mode,
        object requestPayload,
        CancellationToken cancellationToken = default)
    {
        var (systemPrompt, userPayload) = DentalGemmaModeRouter.BuildMessages(mode, requestPayload);
        var rawOutput = await _api.CompleteChatAsync(systemPrompt, userPayload, cancellationToken);
        var parsedJson = DentalGemmaResponseParser.ExtractJson(rawOutput);
        var safety = DentalGemmaSafetyValidator.Validate(rawOutput, mode);

        var audit = new DgAuditRecord
        {
            ModelId = _options.ModelId,
            Mode = DentalGemmaModeNames.ToApiValue(mode),
            PromptTemplateVersion = _options.PromptTemplateVersion,
            RawOutput = rawOutput,
            ParsedJson = parsedJson,
            Safety = safety,
            Timestamp = DateTime.UtcNow
        };

        var result = new DgInvocationResult
        {
            Mode = mode,
            Audit = audit
        };

        if (parsedJson is not null)
        {
            switch (mode)
            {
                case DentalGemmaMode.GeneralChat:
                    result.GeneralChat = JsonSerializer.Deserialize<DgGeneralChatResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? BuildFallbackGeneralChat(rawOutput);
                    break;
                case DentalGemmaMode.ImageQa:
                    result.ImageQa = JsonSerializer.Deserialize<DgImageQaResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? BuildFallbackImageQa(rawOutput);
                    break;
                case DentalGemmaMode.StructuredReport:
                    result.StructuredReport = JsonSerializer.Deserialize<DgStructuredReportResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? BuildFallbackStructuredReport(rawOutput);
                    break;
                case DentalGemmaMode.OrthodonticCaseExplanation:
                    result.OrthodonticCaseExplanation = JsonSerializer.Deserialize<DgOrthodonticCaseExplanationResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? BuildFallbackOrthodontic(rawOutput);
                    break;
                case DentalGemmaMode.ConsistencyCheck:
                    result.ConsistencyCheck = JsonSerializer.Deserialize<DgConsistencyCheckResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? new DgConsistencyCheckResponse { Consistent = false };
                    break;
                case DentalGemmaMode.PatientExplanation:
                    result.PatientExplanation = JsonSerializer.Deserialize<DgPatientExplanationResponse>(parsedJson, DentalGemmaJson.Options)
                        ?? BuildFallbackPatientExplanation(rawOutput);
                    break;
            }
        }
        else
        {
            ApplyTextFallback(result, mode, rawOutput);
        }

        return result;
    }

    public DgOrthodonticCaseExplanationRequest BuildOrthodonticRequestFromXRayResults(XrAnalysisResults results)
    {
        var panoramic = results.Panoramic;
        var summary = panoramic?.Summary ?? new Dictionary<string, int>();

        return new DgOrthodonticCaseExplanationRequest
        {
            CephSummary = new DgCephSummaryInput
            {
                SkeletalClass = results.CephSummary?.SkeletalClass ?? "undetermined",
                Etiology = results.CephSummary?.Etiology ?? "undetermined",
                VerticalPattern = results.CephSummary?.VerticalPattern ?? "undetermined",
                IncisorCompensation = results.CephSummary?.IncisorCompensation ?? "not assessed"
            },
            CvmSummary = new DgCvmSummaryInput
            {
                Stage = results.Cvm?.Stage ?? "unknown",
                GrowthCategory = results.Cvm?.GrowthCategory ?? "unknown"
            },
            PanoramicSummary = new DgPanoramicSummaryInput
            {
                Caries = GetSummaryCount(summary, "caries"),
                ImpactedTooth = GetSummaryCount(summary, "impacted_tooth"),
                RootStump = GetSummaryCount(summary, "root_stump")
            }
        };
    }

    public DgStructuredReportRequest BuildStructuredReportRequest(
        XrAnalysisResults results,
        DgPatientContext patientContext)
    {
        return new DgStructuredReportRequest
        {
            ImageIds = ["img_001"],
            ImageType = "panoramic_xray",
            PatientContext = patientContext,
            DetectorOutputs = MapDetections(results.Panoramic?.Detections ?? [])
        };
    }

    public DgImageQaRequest BuildImageQaRequest(
        XrAnalysisResults results,
        string question,
        string imageType = "panoramic_xray")
    {
        return new DgImageQaRequest
        {
            ImageId = "img_001",
            ImageType = imageType,
            Question = question,
            DetectorOutputs = MapDetections(results.Panoramic?.Detections ?? [])
        };
    }

    private static List<DgDetectorOutput> MapDetections(IEnumerable<XrPanoramicDetection> detections) =>
        detections.Select(d => new DgDetectorOutput
        {
            Class = d.Finding,
            Region = d.Region,
            Confidence = d.Confidence
        }).ToList();

    private static int GetSummaryCount(Dictionary<string, int> summary, string key) =>
        summary.TryGetValue(key, out var count) ? count : 0;

    private static void ApplyTextFallback(DgInvocationResult result, DentalGemmaMode mode, string rawOutput)
    {
        switch (mode)
        {
            case DentalGemmaMode.GeneralChat:
                result.GeneralChat = BuildFallbackGeneralChat(rawOutput);
                break;
            case DentalGemmaMode.ImageQa:
                result.ImageQa = BuildFallbackImageQa(rawOutput);
                break;
            case DentalGemmaMode.StructuredReport:
                result.StructuredReport = BuildFallbackStructuredReport(rawOutput);
                break;
            case DentalGemmaMode.OrthodonticCaseExplanation:
                result.OrthodonticCaseExplanation = BuildFallbackOrthodontic(rawOutput);
                break;
            case DentalGemmaMode.PatientExplanation:
                result.PatientExplanation = BuildFallbackPatientExplanation(rawOutput);
                break;
        }
    }

    private static DgGeneralChatResponse BuildFallbackGeneralChat(string text) => new()
    {
        Answer = text.Trim(),
        RequiresClinicianReview = true
    };

    private static DgImageQaResponse BuildFallbackImageQa(string text) => new()
    {
        Summary = text.Trim(),
        RequiresClinicianReview = true
    };

    private static DgStructuredReportResponse BuildFallbackStructuredReport(string text) => new()
    {
        RadiographicSummary = text.Trim(),
        RequiresClinicianReview = true
    };

    private static DgOrthodonticCaseExplanationResponse BuildFallbackOrthodontic(string text) => new()
    {
        CaseSummary = text.Trim(),
        RequiresClinicianReview = true
    };

    private static DgPatientExplanationResponse BuildFallbackPatientExplanation(string text) => new()
    {
        PatientSummary = text.Trim(),
        RequiresClinicianReview = false
    };
}
