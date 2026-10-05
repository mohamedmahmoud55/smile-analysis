using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmileAnalysisBl.GummySmile.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GsCaseStatus
{
    Created,
    ImagesUploaded,
    Analyzed,
    ClinicalEntered,
    Diagnosed,
    Planned
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GsImageRole
{
    Rest,
    Smile
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GsSex
{
    Male,
    Female
}

public class GsPatientInfo
{
    public string? Name { get; set; }
    public string? RecordId { get; set; }
    public int? Age { get; set; }
    public GsSex Sex { get; set; } = GsSex.Female;
}

public class GsNorms
{
    public double GdIdealMm { get; set; } = 0.0;
    public double GdNormalMaxMm { get; set; } = 3.0;
    public double SeverityMildMaxMm { get; set; } = 4.0;
    public double SeverityModerateMaxMm { get; set; } = 8.0;
    public double LipLengthMaleMm { get; set; } = 23.0;
    public double LipLengthFemaleMm { get; set; } = 21.0;
    public double LipPhiltrumCommissureDiffMinMm { get; set; } = 1.0;
    public double LipPhiltrumCommissureDiffMaxMm { get; set; } = 3.0;
    public double LipMobilityNormPct { get; set; } = 27.0;
    public double CrownWidthLengthRatio { get; set; } = 0.8;
}

public class GsCreateCaseRequest
{
    public GsPatientInfo? Patient { get; set; }
    public GsNorms? Norms { get; set; }
}

public class GsClinicalInputs
{
    public Dictionary<string, double>? ProbingDepthsMm { get; set; }
    public double? U1FeopMm { get; set; }
    public double? SnU1Deg { get; set; }
    public double? NfcADeg { get; set; }
    public double? FacialAxisDeg { get; set; }
    public double? CrownWidthMm { get; set; }
    public double? CrownLengthMm { get; set; }
}

public class GsClinicalUpdateRequest
{
    public GsClinicalInputs Clinical { get; set; } = new();
    public GsPatientInfo? Patient { get; set; }
    public GsNorms? Norms { get; set; }
}

public class GsPoint2D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double V { get; set; } = 1.0;
}

public class GsManualScale
{
    public GsImageRole Role { get; set; }
    public GsPoint2D P1 { get; set; } = new();
    public GsPoint2D P2 { get; set; } = new();
    public double KnownDistanceMm { get; set; }
}

public class GsToothGdOverride
{
    public int? Fdi { get; set; }
    public string? SegClass { get; set; }
    public string? Side { get; set; }
    public double GdMm { get; set; }
}

public class GsOverridesRequest
{
    public GsManualScale? Scale { get; set; }
    public bool Recompute { get; set; } = true;
    public List<GsToothGdOverride>? TeethGd { get; set; }
}

public class GsToothGd
{
    public int? Fdi { get; set; }
    public string SegClass { get; set; } = string.Empty;
    public string Side { get; set; } = "unknown";
    public double GdMm { get; set; }
    public double? ProbingMm { get; set; }
}

public class GsEtiologyContribution
{
    public string Etiology { get; set; } = string.Empty;
    public double AmountMm { get; set; }
    public string Scope { get; set; } = "global";
    public string? Rationale { get; set; }
}

public class GsToothDiagnosis
{
    public int? Fdi { get; set; }
    public string SegClass { get; set; } = string.Empty;
    public string Side { get; set; } = "unknown";
    public double GdMm { get; set; }
    public List<GsEtiologyContribution> Contributions { get; set; } = [];
    public double ExplainedMm { get; set; }
    public bool ResidualFlag { get; set; }
}

public class GsCropResult
{
    public string? CropPath { get; set; }
    public double ClassConfidence { get; set; }
}

public class GsScaleCalibration
{
    public double? PxPerMm { get; set; }
    public string Method { get; set; } = "none";
    public double Confidence { get; set; }
}

public class GsImageAnalysis
{
    public GsImageRole Role { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public GsScaleCalibration Scale { get; set; } = new();
    public GsCropResult Crop { get; set; } = new();
    public string? OverlayPath { get; set; }
    public List<string> Warnings { get; set; } = [];
}

public class GsLipMeasurements
{
    public double? LengthPhiltrumMm { get; set; }
    public double? LengthCommissureMm { get; set; }
    public double? PhiltrumCommissureDiffMm { get; set; }
}

public class GsCrownMeasurement
{
    public string ToothClass { get; set; } = string.Empty;
    public string Side { get; set; } = "unknown";
    public double? WidthMm { get; set; }
    public double? LengthMm { get; set; }
    public double? Ratio { get; set; }
}

public class GsDiagnosisResult
{
    public double ShortLipMm { get; set; }
    public bool ShortLipPresent { get; set; }
    public double HypermobilityMm { get; set; }
    public bool HypermobilityPresent { get; set; }
    public double ExtrusionMm { get; set; }
    public bool ExtrusionPresent { get; set; }
    public bool ProclinationPresent { get; set; }
    public bool RetroclinationPresent { get; set; }
    public bool VmePresent { get; set; }
    public string? FacialPattern { get; set; }
    public double? CrownRatio { get; set; }
    public string? CrownRatioNote { get; set; }
    public string? Severity { get; set; }
    public string? Distribution { get; set; }
    public Dictionary<string, double> Summary { get; set; } = new();
    public List<GsToothDiagnosis> Teeth { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class GsTreatmentItem
{
    public string Etiology { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public List<int> FdiList { get; set; } = [];
    public double InvolvementMm { get; set; }
    public string ModalityKey { get; set; } = string.Empty;
    public string ModalityName { get; set; } = string.Empty;
    public double MeanCorrectionMm { get; set; }
    public double RelapsePct { get; set; }
    public double OvercorrectionMm { get; set; }
    public double TotalCorrectionRequiredMm { get; set; }
    public bool Feasible { get; set; } = true;
    public double ResidualMm { get; set; }
    public string Suitability { get; set; } = "I";
    public string? Rationale { get; set; }
    public double? DegreesRequired { get; set; }
    public List<string> Warnings { get; set; } = [];
}

public class GsTreatmentPlan
{
    public List<GsTreatmentItem> Items { get; set; } = [];
    public List<string> Sequencing { get; set; } = [];
    public List<string> GeneralNotes { get; set; } = [];
    public List<string> Alternatives { get; set; } = [];
}

public class GsMeasurements
{
    public GsLipMeasurements? RestLip { get; set; }
    public GsLipMeasurements? SmileLip { get; set; }
    public double? LipMobilityMm { get; set; }
    public double? LipMobilityPct { get; set; }
    public double? HypermobilityMm { get; set; }
    public GsCrownMeasurement? SelectedCrown { get; set; }
    public List<GsToothGd> TeethGd { get; set; } = [];
    public double? MaxGdMm { get; set; }
    public string? Severity { get; set; }
    public string? Distribution { get; set; }
    public List<string> Notes { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class GsCaseDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public GsCaseStatus Status { get; set; } = GsCaseStatus.Created;
    public GsPatientInfo? Patient { get; set; }
    public GsNorms? Norms { get; set; }
    public GsClinicalInputs? Clinical { get; set; }
    public GsMeasurements? Measurements { get; set; }
    public GsDiagnosisResult? Diagnosis { get; set; }
    public GsTreatmentPlan? Treatment { get; set; }
    public Dictionary<string, GsImageAnalysis>? Images { get; set; }
}

public class GsHealthResponse
{
    public string? Status { get; set; }
}

public static class GummySmileJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
}
