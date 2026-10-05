namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class PatientDetailsPageViewModel
{
    public int Id { get; set; }
    public string DisplayId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarClass { get; set; } = "cc-avatar-blue";
    public int Age { get; set; }
    public string GenderDisplay { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string LastVisitDisplay { get; set; } = "—";
    public string NextAppointmentDisplay { get; set; } = "Not scheduled";
    public int TotalAppointments { get; set; }
    public int CompletedAnalyses { get; set; }
    public int GummySmileCount { get; set; }
    public int XRayCount { get; set; }
    public IReadOnlyList<PreviousAnalysisViewModel> GummySmileAnalyses { get; set; } = [];
    public IReadOnlyList<PreviousAnalysisViewModel> XRayAnalyses { get; set; } = [];
    public IReadOnlyList<PatientAppointmentHistoryViewModel> AppointmentHistory { get; set; } = [];
    public IReadOnlyList<PatientPrescriptionHistoryViewModel> PrescriptionHistory { get; set; } = [];
    public PatientClinicalNotesViewModel ClinicalNotes { get; set; } = new();
}

public class PreviousAnalysisViewModel
{
    public int Id { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string ThumbnailPlaceholder { get; set; } = "GS";
    public string ThumbnailClass { get; set; } = "pa-thumb-gummy";
    public DateTime AnalysisDate { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public string StatusBadgeClass { get; set; } = "pa-badge-success";
    public int? ConfidencePercent { get; set; }
}
