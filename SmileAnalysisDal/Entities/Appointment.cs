using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisDal.Entities;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }

    public DateTime AppointmentTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public Prescription? Prescription { get; set; }
}
