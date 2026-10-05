using System.Text.Json;
using System.Text.Json.Serialization;
using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.XRayAnalysis.Models;

public class XrHealthResponse
{
    public string? Status { get; set; }
}

public class XrPoint2D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double V { get; set; } = 1.0;
}

public class XrPanoramicDetection
{
    public string Finding { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Region { get; set; } = "unspecified region";
    public string ClinicalAction { get; set; } = "Requires clinician review.";
}

public class XrToothLevelFinding
{
    public string Tooth { get; set; } = string.Empty;
    public string Finding { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class XrPanoramicAnalysis
{
    public Dictionary<string, int> Summary { get; set; } = new();
    public List<XrPanoramicDetection> Detections { get; set; } = [];
    public List<XrToothLevelFinding> ToothLevelFindings { get; set; } = [];
    public bool BlockedUntilDentalClearance { get; set; }
    public bool PoorPrognosisToothFlag { get; set; }
    public bool ImpactedToothManagementRequired { get; set; }
    public List<string> Warnings { get; set; } = [];
    public string? OverlayImageBase64 { get; set; }
}

public class XrCvmResult
{
    public string? Stage { get; set; }
    public string GrowthCategory { get; set; } = "unknown";
    public string TreatmentTiming { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<int> RoiBbox { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public string? OverlayImageBase64 { get; set; }
}

public class XrCephLandmarkPrediction
{
    public string Name { get; set; } = string.Empty;
    public XrPoint2D Point { get; set; } = new();
    public double Confidence { get; set; }
}

public class XrCephLandmarkAnalysis
{
    public List<XrCephLandmarkPrediction> Landmarks { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public string? OverlayImageBase64 { get; set; }
}

public class XrCephalometricMeasurements
{
    public double? SnaDeg { get; set; }
    public double? SnbDeg { get; set; }
    public double? AnbDeg { get; set; }
    public double? SnMpDeg { get; set; }
    public double? FmaDeg { get; set; }
    public double? U1SnDeg { get; set; }
    public double? ImpaDeg { get; set; }
    public double? PalatalPlaneSnDeg { get; set; }
    public double? OcclusalPlaneSnDeg { get; set; }
    public double? UpperLipELineMm { get; set; }
    public double? LowerLipELineMm { get; set; }
}

public class XrCephalometricAnalysisRequest
{
    public Dictionary<string, XrPoint2D> Landmarks { get; set; } = new();
    public List<int> RoiBbox { get; set; } = [];
    public string? CvmStage { get; set; }
}

public class XrCephalometricSummary
{
    public XrCephalometricMeasurements Measurements { get; set; } = new();
    public string SkeletalClass { get; set; } = "undetermined";
    public string Etiology { get; set; } = "undetermined";
    public string SkeletalDiscrepancy { get; set; } = "undetermined";
    public string VerticalPattern { get; set; } = "undetermined";
    public string IncisorCompensation { get; set; } = "not assessed";
    public string SoftTissueProfile { get; set; } = "not assessed";
    public List<string> Warnings { get; set; } = [];
    public string? OverlayImageBase64 { get; set; }
}

public class XrTreatmentPlanLogic
{
    public bool BlockedUntilDentalClearance { get; set; }
    public bool GrowthModificationCandidate { get; set; }
    public bool SurgeryFlag { get; set; }
    public string MainRecommendation { get; set; } = string.Empty;
    public List<string> BiomechanicalConsiderations { get; set; } = [];
}

public class XrFullCaseAnalysisRequest
{
    public XrCephalometricSummary CephalometricSummary { get; set; } = new();
    public XrCvmResult CvmSummary { get; set; } = new();
    public XrPanoramicAnalysis PanoramicFindings { get; set; } = new();
    public List<string> PeriodontalBlockers { get; set; } = [];
}

public class XrFullCaseAnalysis
{
    public XrCephalometricSummary CephalometricDiagnosis { get; set; } = new();
    public XrCvmResult Cvm { get; set; } = new();
    public XrPanoramicAnalysis PanoramicFindings { get; set; } = new();
    public XrTreatmentPlanLogic TreatmentPlanLogic { get; set; } = new();
    public List<string> Warnings { get; set; } = [];
}

public class XrCaseContext
{
    public int? Age { get; set; }
    public string? Sex { get; set; }
}

public class XrDentalGemmaReport
{
    public string RadiographicSummary { get; set; } = string.Empty;
    public string PatientFriendlySummary { get; set; } = string.Empty;
}

public class XrDentalGemmaReportRequest
{
    public List<XrPanoramicDetection> PanoramicDetections { get; set; } = [];
    public XrCephalometricSummary CephalometricSummary { get; set; } = new();
    public XrCvmResult CvmSummary { get; set; } = new();
    public XrCaseContext? CaseContext { get; set; }
}

public class XrDentalGemmaReportResponse
{
    public XrDentalGemmaReport ReportJson { get; set; } = new();
    public XrTreatmentPlanLogic TreatmentPlanLogic { get; set; } = new();
    public DgOrthodonticCaseExplanationResponse? OrthodonticExplanation { get; set; }
    public DgAuditRecord? Audit { get; set; }
}

public class XrAnalysisResults
{
    public XrPanoramicAnalysis? Panoramic { get; set; }
    public XrCvmResult? Cvm { get; set; }
    public XrCephLandmarkAnalysis? CephLandmarks { get; set; }
    public XrCephalometricSummary? CephSummary { get; set; }
    public XrFullCaseAnalysis? FullCase { get; set; }
    public XrDentalGemmaReportResponse? DentalReport { get; set; }
    public DateTime? CompletedAt { get; set; }

    public static XrAnalysisResults? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<XrAnalysisResults>(json, XRayAnalysisJson.Options);
    }

    public string Serialize() => JsonSerializer.Serialize(this, XRayAnalysisJson.Options);
}

public static class XRayAnalysisJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
