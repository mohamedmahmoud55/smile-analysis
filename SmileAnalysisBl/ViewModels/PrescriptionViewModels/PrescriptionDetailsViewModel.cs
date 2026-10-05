namespace SmileAnalysisBl.ViewModels.PrescriptionViewModels;

public class PrescriptionDetailsViewModel
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientDisplayId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<PrescriptionItemDetailsViewModel> Items { get; set; } = [];
}

public class PrescriptionItemDetailsViewModel
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}
