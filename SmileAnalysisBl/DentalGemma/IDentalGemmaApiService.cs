using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.DentalGemma;

public interface IDentalGemmaApiService
{
    Task<string> CompleteChatAsync(
        string systemPrompt,
        string userPayload,
        CancellationToken cancellationToken = default);

    Task<DgHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default);
}

public class DgHealthResponse
{
    public string? Status { get; set; }
}
