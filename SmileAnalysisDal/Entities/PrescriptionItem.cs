namespace SmileAnalysisDal.Entities;

public class PrescriptionItem : BaseEntity
{
    public int PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;

    public string MedicationName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
    public string Duration { get; set; } = null!;
    public string? Instructions { get; set; }
}
