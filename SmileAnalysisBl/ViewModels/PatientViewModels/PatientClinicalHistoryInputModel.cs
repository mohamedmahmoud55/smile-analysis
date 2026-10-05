using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.PatientViewModels;

/// <summary>All fields optional (nullable).</summary>
public class PatientClinicalHistoryInputModel
{
    [Display(Name = "Chronic diseases (e.g. diabetes, hypertension)")]
    [StringLength(4000)]
    public string? ChronicDiseases { get; set; }

    [Display(Name = "Current medications")]
    [StringLength(4000)]
    public string? CurrentMedications { get; set; }

    [Display(Name = "Allergies (especially to anesthesia or antibiotics)")]
    [StringLength(4000)]
    public string? Allergies { get; set; }

    [Display(Name = "Previous surgeries")]
    [StringLength(4000)]
    public string? PreviousSurgeries { get; set; }

    [Display(Name = "Heart conditions or bleeding disorders")]
    [StringLength(4000)]
    public string? HeartOrBleedingConditions { get; set; }

    [Display(Name = "Last dental visit")]
    [DataType(DataType.Date)]
    public DateTime? LastDentalVisit { get; set; }

    [Display(Name = "Current pain (location, intensity, duration)")]
    [StringLength(4000)]
    public string? CurrentPainDetails { get; set; }

    [Display(Name = "Previous treatments (fillings, root canals, braces, extractions)")]
    [StringLength(4000)]
    public string? PreviousDentalTreatments { get; set; }

    [Display(Name = "Gum problems (bleeding, swelling)")]
    [StringLength(4000)]
    public string? GumProblems { get; set; }

    [Display(Name = "Habits (smoking, teeth grinding, poor brushing)")]
    [StringLength(4000)]
    public string? OralHabits { get; set; }
}
