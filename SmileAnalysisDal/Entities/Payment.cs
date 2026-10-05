using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisDal.Entities;

public class Payment : BaseEntity
{
    /// <summary>Optional; standalone clinic payments may omit an appointment.</summary>
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
}
