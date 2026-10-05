namespace SmileAnalysisBl.GummySmile;

public static class GsEtiologyCatalog
{
    public static readonly string[] StackOrder =
    [
        "short_lip",
        "hypermobility",
        "gingival_excess",
        "extrusion",
        "retroclination",
        "vme"
    ];

    private static readonly Dictionary<string, (string Label, string Color)> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["short_lip"] = ("Short lip", "#4285F4"),
        ["hypermobility"] = ("Lip hypermobility", "#34A853"),
        ["gingival_excess"] = ("Gingival excess", "#EA4335"),
        ["extrusion"] = ("Incisor extrusion", "#9C27B0"),
        ["retroclination"] = ("Retroclination", "#FF9800"),
        ["vme"] = ("VME", "#FBBC05")
    };

    public static string Label(string etiology) =>
        Map.TryGetValue(etiology, out var entry) ? entry.Label : etiology.Replace('_', ' ');

    public static string Color(string etiology) =>
        Map.TryGetValue(etiology, out var entry) ? entry.Color : "#94a3b8";

    public static string SeverityColor(string? severity) => severity?.ToLowerInvariant() switch
    {
        "severe" => "#dc2626",
        "moderate" => "#ea580c",
        "mild" => "#ca8a04",
        _ => "#16a34a"
    };
}
