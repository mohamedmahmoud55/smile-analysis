using SmileAnalysisBl.XRayAnalysis.Models;

namespace SmileAnalysisBl.XRayAnalysis;

public static class XRayDisplayHelpers
{
    public static IReadOnlyList<XrMeasurementTableRow> BuildCephMeasurementRows(XrCephalometricSummary? summary)
    {
        if (summary?.Measurements is null)
            return [];

        var m = summary.Measurements;
        var rows = new List<XrMeasurementTableRow>();

        AddDeg(rows, "SNA", m.SnaDeg, "82° ± 2°");
        AddDeg(rows, "SNB", m.SnbDeg, "80° ± 2°");
        AddDeg(rows, "ANB", m.AnbDeg, "2° ± 2°");
        AddDeg(rows, "SN–MP (mandibular plane)", m.SnMpDeg, "32° ± 4°");
        AddDeg(rows, "FMA", m.FmaDeg, "25° ± 4°");
        AddDeg(rows, "U1–SN", m.U1SnDeg, "102° ± 6°");
        AddDeg(rows, "IMPA", m.ImpaDeg, "90° ± 3°");
        AddDeg(rows, "Palatal plane–SN", m.PalatalPlaneSnDeg, "—");
        AddDeg(rows, "Occlusal plane–SN", m.OcclusalPlaneSnDeg, "—");
        AddMm(rows, "Upper lip to E-line", m.UpperLipELineMm, "−4 to −2 mm");
        AddMm(rows, "Lower lip to E-line", m.LowerLipELineMm, "−4 to −2 mm");

        return rows;
    }

    public static XrCephalometricSummary? ResolveCephSummary(XrAnalysisResults? results)
    {
        if (results is null) return null;
        if (results.CephSummary is not null) return results.CephSummary;
        return results.FullCase?.CephalometricDiagnosis;
    }

    private static void AddDeg(List<XrMeasurementTableRow> rows, string label, double? value, string norm)
    {
        if (!value.HasValue) return;
        rows.Add(new XrMeasurementTableRow
        {
            Assessment = label,
            Measured = $"{value.Value:0.#}°",
            Norm = norm
        });
    }

    private static void AddMm(List<XrMeasurementTableRow> rows, string label, double? value, string norm)
    {
        if (!value.HasValue) return;
        rows.Add(new XrMeasurementTableRow
        {
            Assessment = label,
            Measured = $"{value.Value:0.#} mm",
            Norm = norm
        });
    }
}

public class XrMeasurementTableRow
{
    public string Assessment { get; set; } = string.Empty;
    public string Measured { get; set; } = string.Empty;
    public string Norm { get; set; } = "—";
}
