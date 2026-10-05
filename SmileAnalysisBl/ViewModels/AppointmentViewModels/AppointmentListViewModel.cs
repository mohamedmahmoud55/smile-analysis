using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisBl.ViewModels.AppointmentViewModels;

public class AppointmentListViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public DateTime AppointmentTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public bool HasPrescription { get; set; }
}
