using SmileAnalysisBl.ViewModels.AppointmentViewModels;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class DoctorSchedulePageViewModel
{
    public int DoctorId { get; set; }
    public string DoctorDisplayName { get; set; } = string.Empty;

    /// <summary>Inclusive start of the filtered range (for the date picker).</summary>
    public DateOnly RangeStart { get; set; }

    /// <summary>Inclusive end of the filtered range (for the date picker).</summary>
    public DateOnly RangeEnd { get; set; }

    public IReadOnlyList<AppointmentListViewModel> Appointments { get; set; } = [];
}
