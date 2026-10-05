using System.ComponentModel.DataAnnotations;
using SmileAnalysisBl.GummySmile;
using SmileAnalysisBl.GummySmile.Models;

namespace SmileAnalysisBl.ViewModels.GummySmileViewModels;

public enum GummySmileWizardStep
{
    Intake = 1,
    Review = 2,
    Clinical = 3,
    Diagnosis = 4,
    Treatment = 5
}

public class GummySmileWizardBaseViewModel
{
    public int LocalCaseId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string GsCaseId { get; set; } = string.Empty;
    public GummySmileWizardStep CurrentStep { get; set; }
    public string GsStatus { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int PatientAge { get; set; }
    public string PatientSex { get; set; } = string.Empty;
    public GsWizardStepStatus StepStatus { get; set; } = new();
}

public class GummySmileUploadViewModel : GummySmileWizardBaseViewModel
{
    public bool RestImageUploaded { get; set; }
    public bool SmileImageUploaded { get; set; }
    public bool AnalyzeCompleted { get; set; }
}

public class GummySmileAnalyzeViewModel : GummySmileWizardBaseViewModel
{
    public bool RestImageUploaded { get; set; }
    public bool SmileImageUploaded { get; set; }
}

public class GummySmileGdOverrideRow
{
    public int? Fdi { get; set; }
    public string SegClass { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public double AiGdMm { get; set; }

    [Display(Name = "Override GD (mm)")]
    public double? OverrideGdMm { get; set; }
}

public class GummySmileReviewViewModel : GummySmileWizardBaseViewModel
{
    public GsCaseDto? Case { get; set; }
    public GsMeasurements? Measurements { get; set; }
    public List<string> AiWarnings { get; set; } = [];
    public bool LowScaleConfidence { get; set; }
    public List<GummySmileGdOverrideRow> GdOverrides { get; set; } = [];
    public string? RestOverlayPath { get; set; }
    public string? SmileOverlayPath { get; set; }
}

public class GummySmileProbingInput
{
    public string Fdi { get; set; } = string.Empty;
    public string ToothLabel { get; set; } = string.Empty;

    [Display(Name = "Probing depth (mm)")]
    public double? DepthMm { get; set; }
}

public class GummySmileClinicalViewModel : GummySmileWizardBaseViewModel
{
    public List<GummySmileProbingInput> ProbingDepths { get; set; } = [];

    [Display(Name = "U1 FEOP (mm)")]
    public double? U1FeopMm { get; set; }

    [Display(Name = "SN–U1 angle (°)")]
    public double? SnU1Deg { get; set; }

    [Display(Name = "NFC–A angle (°)")]
    public double? NfcADeg { get; set; }

    [Display(Name = "Facial axis (°)")]
    public double? FacialAxisDeg { get; set; }

    [Display(Name = "Crown width (mm)")]
    public double? CrownWidthMm { get; set; }

    [Display(Name = "Crown length (mm)")]
    public double? CrownLengthMm { get; set; }

    public bool HasDraft { get; set; }
}

public class GummySmileDiagnosisViewModel : GummySmileWizardBaseViewModel
{
    public GsCaseDto? Case { get; set; }
    public bool IsReady { get; set; }
    public string? ToothChartJson { get; set; }
    public IReadOnlyList<GsMeasurementTableRow> MeasurementRows { get; set; } = [];
    public string? SmileOverlayPath { get; set; }
    public bool SmileImageIsOriginal { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class GummySmileTreatmentViewModel : GummySmileWizardBaseViewModel
{
    public GsCaseDto? Case { get; set; }
    public bool IsReady { get; set; }
    public bool HasPdf { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public static class GummySmileWizardFdi
{
    public static readonly string[] UpperFdi =
    [
        "18", "17", "16", "15", "14", "13", "12", "11",
        "21", "22", "23", "24", "25", "26", "27", "28"
    ];
}
