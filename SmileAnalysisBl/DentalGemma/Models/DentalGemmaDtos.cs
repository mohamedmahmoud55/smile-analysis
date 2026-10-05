using System.Text.Json.Serialization;

namespace SmileAnalysisBl.DentalGemma.Models;

public enum DentalGemmaMode
{
    GeneralChat,
    ImageQa,
    StructuredReport,
    OrthodonticCaseExplanation,
    ConsistencyCheck,
    PatientExplanation
}

public static class DentalGemmaModeNames
{
    public const string GeneralChat = "general_chat";
    public const string ImageQa = "image_qa";
    public const string StructuredReport = "structured_report";
    public const string OrthodonticCaseExplanation = "orthodontic_case_explanation";
    public const string ConsistencyCheck = "consistency_check";
    public const string PatientExplanation = "patient_explanation";

    public static string ToApiValue(DentalGemmaMode mode) => mode switch
    {
        DentalGemmaMode.GeneralChat => GeneralChat,
        DentalGemmaMode.ImageQa => ImageQa,
        DentalGemmaMode.StructuredReport => StructuredReport,
        DentalGemmaMode.OrthodonticCaseExplanation => OrthodonticCaseExplanation,
        DentalGemmaMode.ConsistencyCheck => ConsistencyCheck,
        DentalGemmaMode.PatientExplanation => PatientExplanation,
        _ => GeneralChat
    };

    public static DentalGemmaMode Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        GeneralChat => DentalGemmaMode.GeneralChat,
        ImageQa => DentalGemmaMode.ImageQa,
        StructuredReport => DentalGemmaMode.StructuredReport,
        OrthodonticCaseExplanation => DentalGemmaMode.OrthodonticCaseExplanation,
        ConsistencyCheck => DentalGemmaMode.ConsistencyCheck,
        PatientExplanation => DentalGemmaMode.PatientExplanation,
        _ => DentalGemmaMode.GeneralChat
    };
}

// --- Mode requests ---

public class DgGeneralChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object?> CaseContext { get; set; } = new();
    public string SafetyProfile { get; set; } = "educational";
}

public class DgDetectorOutput
{
    public string Class { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<int> Bbox { get; set; } = [];
}

public class DgImageQaRequest
{
    public string ImageId { get; set; } = string.Empty;
    public string ImageType { get; set; } = "panoramic_xray";
    public string Question { get; set; } = string.Empty;
    public List<DgDetectorOutput> DetectorOutputs { get; set; } = [];
}

public class DgPatientContext
{
    public int? Age { get; set; }
    public string? Sex { get; set; }
    public string? ChiefComplaint { get; set; }
}

public class DgStructuredReportRequest
{
    public List<string> ImageIds { get; set; } = [];
    public string ImageType { get; set; } = "panoramic_xray";
    public DgPatientContext PatientContext { get; set; } = new();
    public List<DgDetectorOutput> DetectorOutputs { get; set; } = [];
}

public class DgCephSummaryInput
{
    public string SkeletalClass { get; set; } = string.Empty;
    public string Etiology { get; set; } = string.Empty;
    public string VerticalPattern { get; set; } = string.Empty;
    public string IncisorCompensation { get; set; } = string.Empty;
}

public class DgCvmSummaryInput
{
    public string Stage { get; set; } = string.Empty;
    public string GrowthCategory { get; set; } = string.Empty;
}

public class DgPanoramicSummaryInput
{
    public int Caries { get; set; }
    public int ImpactedTooth { get; set; }
    public int RootStump { get; set; }
}

public class DgOrthodonticCaseExplanationRequest
{
    public DgCephSummaryInput CephSummary { get; set; } = new();
    public DgCvmSummaryInput CvmSummary { get; set; } = new();
    public DgPanoramicSummaryInput PanoramicSummary { get; set; } = new();
}

public class DgConsistencyCheckRequest
{
    public Dictionary<string, object?> RuleEngineOutput { get; set; } = new();
    public Dictionary<string, object?> DentalGemmaReport { get; set; } = new();
}

public class DgPatientExplanationRequest
{
    public Dictionary<string, object?> ApprovedFindings { get; set; } = new();
    public string Tone { get; set; } = "simple";
    public string Language { get; set; } = "English";
}

// --- Mode responses ---

public class DgGeneralChatResponse
{
    public string Mode { get; set; } = DentalGemmaModeNames.GeneralChat;
    public string Answer { get; set; } = string.Empty;
    public bool RequiresClinicianReview { get; set; } = true;
    public bool Blocked { get; set; }
}

public class DgPossibleFinding
{
    public string Finding { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ConfidenceLanguage { get; set; } = "possible";
    public string RecommendedConfirmation { get; set; } = string.Empty;
}

public class DgImageQaResponse
{
    public string Mode { get; set; } = DentalGemmaModeNames.ImageQa;
    public string Summary { get; set; } = string.Empty;
    public List<DgPossibleFinding> PossibleFindings { get; set; } = [];
    public bool RequiresClinicianReview { get; set; } = true;
}

public class DgStructuredFinding
{
    public string Finding { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string OrthodonticRelevance { get; set; } = string.Empty;
    public string FollowUp { get; set; } = string.Empty;
    public string Urgency { get; set; } = "low";
}

public class DgStructuredReportResponse
{
    public string Mode { get; set; } = DentalGemmaModeNames.StructuredReport;
    public string RadiographicSummary { get; set; } = string.Empty;
    public List<DgStructuredFinding> Findings { get; set; } = [];
    public List<string> ClinicalLimitations { get; set; } = [];
    public bool RequiresClinicianReview { get; set; } = true;
}

public class DgOrthodonticCaseExplanationResponse
{
    public string CaseSummary { get; set; } = string.Empty;
    public string GrowthTimingComment { get; set; } = string.Empty;
    public List<string> OrthodonticOptionsExplanation { get; set; } = [];
    public List<string> DentalBlockers { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];
    public bool RequiresClinicianReview { get; set; } = true;
}

public class DgConsistencyConflict
{
    public string Field { get; set; } = string.Empty;
    public string RuleEngineValue { get; set; } = string.Empty;
    public string DentalGemmaValue { get; set; } = string.Empty;
    public string Severity { get; set; } = "low";
    public string RecommendedAction { get; set; } = string.Empty;
}

public class DgConsistencyCheckResponse
{
    public bool Consistent { get; set; } = true;
    public List<DgConsistencyConflict> Conflicts { get; set; } = [];
    public bool SafeToShowPatient { get; set; }
}

public class DgPatientExplanationResponse
{
    public string PatientSummary { get; set; } = string.Empty;
    public List<string> WhatThisMeans { get; set; } = [];
    public List<string> NextSteps { get; set; } = [];
    public bool RequiresClinicianReview { get; set; }
}

// --- Safety & audit ---

public class DgSafetyValidationResult
{
    public bool Safe { get; set; }
    public List<string> ForbiddenHits { get; set; } = [];
    public bool HasReviewLanguage { get; set; }
    public bool RequiresClinicianReview { get; set; } = true;
}

public class DgAuditRecord
{
    public string ModelId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string PromptTemplateVersion { get; set; } = string.Empty;
    public string RawOutput { get; set; } = string.Empty;
    public string? ParsedJson { get; set; }
    public DgSafetyValidationResult Safety { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ApprovedContent { get; set; }
}

public class DgInvocationResult
{
    public DentalGemmaMode Mode { get; set; }
    public DgAuditRecord Audit { get; set; } = new();
    public DgGeneralChatResponse? GeneralChat { get; set; }
    public DgImageQaResponse? ImageQa { get; set; }
    public DgStructuredReportResponse? StructuredReport { get; set; }
    public DgOrthodonticCaseExplanationResponse? OrthodonticCaseExplanation { get; set; }
    public DgConsistencyCheckResponse? ConsistencyCheck { get; set; }
    public DgPatientExplanationResponse? PatientExplanation { get; set; }
}

// --- OpenAI-compatible chat API ---

public class DgChatCompletionRequest
{
    public string Model { get; set; } = "dentalgemma";
    public List<DgChatMessage> Messages { get; set; } = [];
    public int MaxTokens { get; set; } = 900;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.1;
}

public class DgChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public class DgChatCompletionResponse
{
    public List<DgChatChoice> Choices { get; set; } = [];
}

public class DgChatChoice
{
    public DgChatMessageResponse Message { get; set; } = new();
}

public class DgChatMessageResponse
{
    public string Content { get; set; } = string.Empty;
}
