using SmileAnalysisBl.DentalGemma.Models;

namespace SmileAnalysisBl.DentalGemma;

public static class DentalGemmaSafetyValidator
{
    private static readonly string[] ForbiddenPatterns =
    [
        "definitively diagnosed",
        "no dentist needed",
        "guaranteed cure",
        "prescribe",
        "take antibiotics",
        "treatment must be"
    ];

    private static readonly string[] RequiredPatterns =
    [
        "clinician",
        "dentist",
        "orthodontist",
        "confirmation",
        "review"
    ];

    public static DgSafetyValidationResult Validate(string text, DentalGemmaMode mode)
    {
        var lower = text.ToLowerInvariant();
        var forbiddenHits = ForbiddenPatterns.Where(p => lower.Contains(p, StringComparison.Ordinal)).ToList();
        var hasReviewLanguage = RequiredPatterns.Any(p => lower.Contains(p, StringComparison.Ordinal));

        var requiresReview = mode != DentalGemmaMode.PatientExplanation;
        var safe = forbiddenHits.Count == 0 && (hasReviewLanguage || mode == DentalGemmaMode.PatientExplanation);

        return new DgSafetyValidationResult
        {
            Safe = safe,
            ForbiddenHits = forbiddenHits,
            HasReviewLanguage = hasReviewLanguage,
            RequiresClinicianReview = requiresReview
        };
    }
}
