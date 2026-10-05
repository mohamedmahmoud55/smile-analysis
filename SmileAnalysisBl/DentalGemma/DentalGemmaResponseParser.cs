using System.Text.Json;
using System.Text.RegularExpressions;
using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.DentalGemma;

public static partial class DentalGemmaResponseParser
{
    public static string? ExtractJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var cleaned = StripThinkingTokens(raw.Trim());

        if (TryParseJsonObject(cleaned, out _))
            return cleaned;

        var fenced = JsonFenceRegex().Match(cleaned);
        if (fenced.Success)
        {
            var inner = fenced.Groups[1].Value.Trim();
            if (TryParseJsonObject(inner, out _))
                return inner;
        }

        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            var candidate = cleaned[start..(end + 1)];
            if (TryParseJsonObject(candidate, out _))
                return candidate;
        }

        return null;
    }

    public static T? Deserialize<T>(string raw) where T : class
    {
        var json = ExtractJson(raw);
        if (json is null) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DentalGemmaJson.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string StripThinkingTokens(string text)
    {
        var idx = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            return text[idx..];

        idx = text.IndexOf('{');
        if (idx > 0)
            return text[idx..];

        return text;
    }

    private static bool TryParseJsonObject(string text, out JsonDocument? doc)
    {
        doc = null;
        try
        {
            doc = JsonDocument.Parse(text);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [GeneratedRegex(@"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase)]
    private static partial Regex JsonFenceRegex();
}
