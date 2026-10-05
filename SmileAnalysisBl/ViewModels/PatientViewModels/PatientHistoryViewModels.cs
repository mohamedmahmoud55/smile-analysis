using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class PatientAppointmentHistoryViewModel
{
    public int Id { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusBadgeClass { get; set; } = string.Empty;
    public bool HasPrescription { get; set; }
    public int? PrescriptionId { get; set; }
}

public class PatientPrescriptionHistoryViewModel
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public int MedicationCount { get; set; }
    public string MedicationSummary { get; set; } = string.Empty;
    public string? NotesPreview { get; set; }
}

public class PatientClinicalNotesViewModel
{
    public bool HasNotes { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? CurrentMedications { get; set; }
    public string? Allergies { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? HeartOrBleedingConditions { get; set; }
    public string? CurrentPainDetails { get; set; }
    public string? PreviousDentalTreatments { get; set; }
    public string? GumProblems { get; set; }
    public string? OralHabits { get; set; }
    public string? LastDentalVisitDisplay { get; set; }
}
