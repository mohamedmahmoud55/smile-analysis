using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.ViewModels.DentalGemmaViewModels;

public class DentalGemmaPageViewModel
{
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? ErrorMessage { get; set; }
    public DgInvocationResult? LastResult { get; set; }
    public string GeneralChatMessage { get; set; } = string.Empty;
}
