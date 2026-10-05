using System.Text.Json;
using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.DentalGemma;

public static class DentalGemmaModeRouter
{
    public static (string SystemPrompt, string UserPayload) BuildMessages(
        DentalGemmaMode mode,
        object requestPayload)
    {
        var systemPrompt = DentalGemmaSystemPrompts.ForMode(mode);
        var payload = new Dictionary<string, object?>
        {
            ["mode"] = DentalGemmaModeNames.ToApiValue(mode)
        };

        switch (mode)
        {
            case DentalGemmaMode.GeneralChat when requestPayload is DgGeneralChatRequest chat:
                payload["message"] = chat.Message;
                payload["case_context"] = chat.CaseContext;
                payload["safety_profile"] = chat.SafetyProfile;
                break;

            case DentalGemmaMode.ImageQa when requestPayload is DgImageQaRequest imageQa:
                payload["image_id"] = imageQa.ImageId;
                payload["image_type"] = imageQa.ImageType;
                payload["question"] = imageQa.Question;
                payload["detector_outputs"] = imageQa.DetectorOutputs;
                break;

            case DentalGemmaMode.StructuredReport when requestPayload is DgStructuredReportRequest report:
                payload["image_ids"] = report.ImageIds;
                payload["image_type"] = report.ImageType;
                payload["patient_context"] = report.PatientContext;
                payload["detector_outputs"] = report.DetectorOutputs;
                break;

            case DentalGemmaMode.OrthodonticCaseExplanation when requestPayload is DgOrthodonticCaseExplanationRequest ortho:
                payload["ceph_summary"] = ortho.CephSummary;
                payload["cvm_summary"] = ortho.CvmSummary;
                payload["panoramic_summary"] = ortho.PanoramicSummary;
                break;

            case DentalGemmaMode.ConsistencyCheck when requestPayload is DgConsistencyCheckRequest consistency:
                payload["rule_engine_output"] = consistency.RuleEngineOutput;
                payload["dentalgemma_report"] = consistency.DentalGemmaReport;
                break;

            case DentalGemmaMode.PatientExplanation when requestPayload is DgPatientExplanationRequest patient:
                payload["approved_findings"] = patient.ApprovedFindings;
                payload["tone"] = patient.Tone;
                payload["language"] = patient.Language;
                break;
        }

        var userPayload = JsonSerializer.Serialize(payload, DentalGemmaJson.Options);
        return (systemPrompt, userPayload);
    }
}
