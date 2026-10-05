namespace SmileAnalysisDal.Entities;

public class Prescription : BaseEntity
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string? Notes { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
