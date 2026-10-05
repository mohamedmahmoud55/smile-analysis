namespace SmileAnalysisBl.DentalGemma;

public class DentalGemmaApiOptions
{
    public const string SectionName = "DentalGemmaApi";

    public string BaseUrl { get; set; } = "https://amrgamal1805--dentalgemma-l4-serve.modal.run";

    public string ModelId { get; set; } = "dentalgemma";

    public int MaxTokens { get; set; } = 900;

    public double Temperature { get; set; } = 0.1;

    public string PromptTemplateVersion { get; set; } = "1.0";
}
