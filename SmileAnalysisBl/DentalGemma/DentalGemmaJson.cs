using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmileAnalysisBl.DentalGemma;

public static class DentalGemmaJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
