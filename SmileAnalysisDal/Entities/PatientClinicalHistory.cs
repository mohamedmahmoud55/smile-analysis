namespace SmileAnalysisDal.Entities;

/// <summary>Optional 1:1 clinical notes for a patient. All clinical fields are nullable.</summary>
public class PatientClinicalHistory : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string? ChronicDiseases { get; set; }
    public string? CurrentMedications { get; set; }
    public string? Allergies { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? HeartOrBleedingConditions { get; set; }

    public DateTime? LastDentalVisit { get; set; }
    public string? CurrentPainDetails { get; set; }
    public string? PreviousDentalTreatments { get; set; }
    public string? GumProblems { get; set; }
    public string? OralHabits { get; set; }
}
