using System.Text.Json;
using SmileAnalysisBl.GummySmile.Models;

namespace SmileAnalysisBl.GummySmile;

public static class GummySmileDisplayHelpers
{
    public static GsWizardStepStatus ComputeStepStatus(GsCaseDto? gsCase)
    {
        if (gsCase is null)
            return new GsWizardStepStatus();

        var hasImages = gsCase.Images is { Count: > 0 };
        var analyzeDone = gsCase.Images?.Values.Any(i => !string.IsNullOrEmpty(i.Crop?.CropPath)) == true;
        var clinicalDone = gsCase.Status is GsCaseStatus.ClinicalEntered or GsCaseStatus.Diagnosed or GsCaseStatus.Planned;
        var diagnosisDone = gsCase.Diagnosis is not null;
        var treatmentDone = gsCase.Treatment?.Items.Count > 0;

        return new GsWizardStepStatus
        {
            IntakeDone = hasImages && analyzeDone,
            ReviewDone = analyzeDone,
            ClinicalDone = clinicalDone,
            DiagnosisDone = diagnosisDone,
            TreatmentDone = treatmentDone
        };
    }

    public static double? SummaryValue(GsDiagnosisResult? diagnosis, string key)
    {
        if (diagnosis?.Summary is null || !diagnosis.Summary.TryGetValue(key, out var value))
            return null;
        return value;
    }

    public static string ToothLabel(GsToothDiagnosis tooth) =>
        tooth.Fdi?.ToString() ?? tooth.SegClass;

    public static IEnumerable<GsToothDiagnosis> ChartTeeth(GsDiagnosisResult? diagnosis)
    {
        if (diagnosis?.Teeth is not { Count: > 0 })
            return [];

        var withGd = diagnosis.Teeth.Where(t => t.GdMm > 0).ToList();
        return withGd.Count > 0 ? withGd : diagnosis.Teeth;
    }

    public static string BuildToothChartJson(GsDiagnosisResult? diagnosis)
    {
        var teeth = ChartTeeth(diagnosis).ToList();
        var labels = teeth.Select(ToothLabel).ToList();
        var datasets = GsEtiologyCatalog.StackOrder.Select(etiology => new
        {
            label = GsEtiologyCatalog.Label(etiology),
            backgroundColor = GsEtiologyCatalog.Color(etiology),
            data = teeth.Select(t =>
                t.Contributions.FirstOrDefault(c => string.Equals(c.Etiology, etiology, StringComparison.OrdinalIgnoreCase))?.AmountMm ?? 0
            ).ToList()
        }).ToList();

        return JsonSerializer.Serialize(new { labels, datasets });
    }

    public static IReadOnlyList<GsMeasurementTableRow> BuildMeasurementTable(GsCaseDto gsCase)
    {
        var m = gsCase.Measurements;
        var n = gsCase.Norms;
        if (m is null) return [];

        var rows = new List<GsMeasurementTableRow>();
        var lipNorm = gsCase.Patient?.Sex == GsSex.Male ? n?.LipLengthMaleMm : n?.LipLengthFemaleMm;

        AddRow(rows, "Rest philtrum length", m.RestLip?.LengthPhiltrumMm, lipNorm is not null ? $"{lipNorm:0.#} mm" : null);
        AddRow(rows, "Rest commissure length", m.RestLip?.LengthCommissureMm, lipNorm is not null ? $"{lipNorm:0.#} mm" : null);
        AddRow(rows, "Rest philtrum–commissure diff", m.RestLip?.PhiltrumCommissureDiffMm,
            n is not null ? $"{n.LipPhiltrumCommissureDiffMinMm:0.#}–{n.LipPhiltrumCommissureDiffMaxMm:0.#} mm" : null);
        AddRow(rows, "Smile philtrum length", m.SmileLip?.LengthPhiltrumMm, null);
        AddRow(rows, "Smile commissure length", m.SmileLip?.LengthCommissureMm, null);
        AddRow(rows, "Smile philtrum–commissure diff", m.SmileLip?.PhiltrumCommissureDiffMm, null);
        AddRow(rows, "Lip mobility", m.LipMobilityMm, n?.LipMobilityNormPct is not null ? $"{n.LipMobilityNormPct:0.#}% ref" : null, "mm");
        AddRow(rows, "Lip mobility (%)", m.LipMobilityPct, n?.LipMobilityNormPct is not null ? $"{n.LipMobilityNormPct:0.#}%" : null, "%");
        AddRow(rows, "Hypermobility", m.HypermobilityMm, null, "mm");
        AddRow(rows, "Max gingival display", m.MaxGdMm, n is not null ? $"≤ {n.GdNormalMaxMm:0.#} mm" : null, "mm");
        AddRow(rows, "Crown width", m.SelectedCrown?.WidthMm, null, "mm");
        AddRow(rows, "Crown length", m.SelectedCrown?.LengthMm, null, "mm");
        AddRow(rows, "Crown W/L ratio", m.SelectedCrown?.Ratio, n is not null ? $"{n.CrownWidthLengthRatio:0.##}" : null);

        return rows.Where(r => r.HasValue).ToList();
    }

    private static void AddRow(List<GsMeasurementTableRow> rows, string assessment, double? measured, string? norm, string unit = "mm")
    {
        if (!measured.HasValue) return;
        rows.Add(new GsMeasurementTableRow
        {
            Assessment = assessment,
            Measured = $"{measured.Value:0.##} {unit}",
            Norm = norm ?? "—"
        });
    }

    public static string? SmileOverlayPath(GsCaseDto gsCase)
    {
        if (gsCase.Images is null) return null;
        if (gsCase.Images.TryGetValue("smile", out var smile))
            return smile.OverlayPath;
        return gsCase.Images.Values.FirstOrDefault(i => i.Role == GsImageRole.Smile)?.OverlayPath;
    }
}

public class GsWizardStepStatus
{
    public bool IntakeDone { get; set; }
    public bool ReviewDone { get; set; }
    public bool ClinicalDone { get; set; }
    public bool DiagnosisDone { get; set; }
    public bool TreatmentDone { get; set; }
}

public class GsMeasurementTableRow
{
    public string Assessment { get; set; } = string.Empty;
    public string Measured { get; set; } = string.Empty;
    public string Norm { get; set; } = "—";
    public bool HasValue => !string.IsNullOrEmpty(Measured);
}
