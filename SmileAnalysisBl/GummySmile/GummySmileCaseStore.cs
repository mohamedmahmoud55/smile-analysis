using System.Text.Json;
using SmileAnalysisBl.GummySmile.Models;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisBl.GummySmile;

public sealed class GsCachedAssetEntry
{
    public string ContentType { get; set; } = "application/octet-stream";
    public string Base64 { get; set; } = string.Empty;
}

public static class GummySmileLocalAssets
{
    public const string Smile = "__uploaded/smile";
    public const string Rest = "__uploaded/rest";

    public static bool IsUploadedPath(string? path) =>
        path is Smile or Rest;

    public static bool IsSmileRelated(string path) =>
        path.Contains("smile", StringComparison.OrdinalIgnoreCase);

    public static bool IsRestRelated(string path) =>
        path.Contains("rest", StringComparison.OrdinalIgnoreCase);
}

public static class GummySmileCaseStore
{
    public static GsCaseDto? DeserializeCase(GummySmileCase localCase)
    {
        if (string.IsNullOrWhiteSpace(localCase.CaseSnapshotJson))
            return null;

        return JsonSerializer.Deserialize<GsCaseDto>(localCase.CaseSnapshotJson, GummySmileJson.Options);
    }

    public static GsCaseDto? TryDeserializeStoredCase(GummySmileCase localCase)
    {
        var fromSnapshot = DeserializeCase(localCase);
        if (fromSnapshot is not null)
            return fromSnapshot;

        if (string.IsNullOrWhiteSpace(localCase.ReportJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<GsCaseDto>(localCase.ReportJson, GummySmileJson.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string SerializeCase(GsCaseDto gsCase) =>
        JsonSerializer.Serialize(gsCase, GummySmileJson.Options);

    public static Dictionary<string, GsCachedAssetEntry> DeserializeAssets(GummySmileCase localCase)
    {
        if (string.IsNullOrWhiteSpace(localCase.CachedAssetsJson))
            return new Dictionary<string, GsCachedAssetEntry>(StringComparer.OrdinalIgnoreCase);

        return JsonSerializer.Deserialize<Dictionary<string, GsCachedAssetEntry>>(
                   localCase.CachedAssetsJson,
                   GummySmileJson.Options)
               ?? new Dictionary<string, GsCachedAssetEntry>(StringComparer.OrdinalIgnoreCase);
    }

    public static string SerializeAssets(Dictionary<string, GsCachedAssetEntry> assets) =>
        JsonSerializer.Serialize(assets, GummySmileJson.Options);

    public static IEnumerable<string> AssetPaths(GsCaseDto gsCase)
    {
        if (gsCase.Images is null)
            yield break;

        foreach (var image in gsCase.Images.Values)
        {
            if (!string.IsNullOrWhiteSpace(image.OverlayPath))
                yield return image.OverlayPath.TrimStart('/');

            if (!string.IsNullOrWhiteSpace(image.Crop?.CropPath))
                yield return image.Crop.CropPath.TrimStart('/');
        }
    }

    public static void ApplySnapshotFields(GummySmileCase localCase, GsCaseDto gsCase)
    {
        localCase.CaseSnapshotJson = SerializeCase(gsCase);
        localCase.GsStatus = gsCase.Status.ToString().ToLowerInvariant();
        localCase.Severity = gsCase.Diagnosis?.Severity ?? gsCase.Measurements?.Severity ?? localCase.Severity;
        if (gsCase.Diagnosis is not null)
            localCase.DiagnosisSummary = BuildDiagnosisSummary(gsCase);
    }

    public static (byte[] Data, string ContentType)? TryGetCachedAsset(GummySmileCase localCase, string assetPath)
    {
        var normalized = assetPath.TrimStart('/');
        var assets = DeserializeAssets(localCase);

        if (!assets.TryGetValue(normalized, out var entry) || string.IsNullOrWhiteSpace(entry.Base64))
            return null;

        try
        {
            return (Convert.FromBase64String(entry.Base64), entry.ContentType);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static void AddCachedAsset(
        Dictionary<string, GsCachedAssetEntry> assets,
        string assetPath,
        byte[] data,
        string contentType)
    {
        var normalized = assetPath.TrimStart('/');
        assets[normalized] = new GsCachedAssetEntry
        {
            ContentType = contentType,
            Base64 = Convert.ToBase64String(data)
        };
    }

    public static (byte[] Data, string ContentType)? TryGetUploadedImage(GummySmileCase localCase, string role)
    {
        if (string.Equals(role, "smile", StringComparison.OrdinalIgnoreCase)
            && localCase.SmileImage is { Length: > 0 })
        {
            return (localCase.SmileImage, localCase.SmileImageContentType ?? "image/jpeg");
        }

        if (string.Equals(role, "rest", StringComparison.OrdinalIgnoreCase)
            && localCase.RestImage is { Length: > 0 })
        {
            return (localCase.RestImage, localCase.RestImageContentType ?? "image/jpeg");
        }

        return null;
    }

    private static string BuildDiagnosisSummary(GsCaseDto gsCase)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(gsCase.Diagnosis?.Severity))
            parts.Add($"{gsCase.Diagnosis.Severity} gummy smile");
        if (gsCase.Measurements?.MaxGdMm is { } gd)
            parts.Add($"{gd:0.#} mm gingival display");
        if (gsCase.Diagnosis?.HypermobilityPresent == true)
            parts.Add("hypermobility detected");
        return parts.Count > 0 ? string.Join(" · ", parts) : "Diagnosis complete";
    }
}
