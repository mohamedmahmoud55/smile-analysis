using SmileAnalysisBl.ViewModels.AppointmentViewModels;
using SmileAnalysisBl.ViewModels.DoctorViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IAppointmentService
{
    IEnumerable<AppointmentListViewModel> GetAll();

    /// <summary>Appointments for a doctor within an inclusive date range (defaults: today → today + 41 days).</summary>
    DoctorSchedulePageViewModel? GetDoctorSchedule(int doctorId, DateOnly? rangeStart = null, DateOnly? rangeEnd = null);
    CreateAppointmentViewModel GetCreateForm(int? preselectPatientId = null);
    bool TryCreate(CreateAppointmentViewModel model, out string? errorMessage);

    EditAppointmentViewModel? GetForEdit(int id);

    /// <summary>Update a non-cancelled appointment. Excludes <paramref name="id"/> from overlap checks.</summary>
    bool TryUpdate(EditAppointmentViewModel model, out string? errorMessage);

    /// <summary>Marks the appointment as cancelled (soft remove).</summary>
    bool TryCancel(int id, out string? errorMessage);

    /// <summary>Start times on <paramref name="date"/> for the doctor; optional <paramref name="excludeAppointmentId"/> ignores that row when checking overlaps (30-minute grid).</summary>
    IReadOnlyList<TimeOnly> GetAvailableSlotStarts(int doctorId, DateOnly date, int durationMinutes, int? excludeAppointmentId = null);
}
