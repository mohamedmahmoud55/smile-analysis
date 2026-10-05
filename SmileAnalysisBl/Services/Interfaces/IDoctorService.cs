using SmileAnalysisBl.ViewModels.DoctorViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IDoctorService
{
    DoctorRecordsPageViewModel GetMedicalTeamPage(string? search = null);
    bool Create(CreateDoctorViewModel model);
    EditDoctorViewModel? GetDoctorForEdit(int id);
    bool Update(EditDoctorViewModel model);
    bool TryDelete(int id, out string? error);

    DoctorAvailabilityPageViewModel? GetAvailabilityPage(int doctorId);
    bool TryAddAvailability(AddDoctorAvailabilityViewModel model, out string? error);
    bool TryDeleteAvailability(int doctorId, int availabilityId, out string? error);

    /// <summary>Resolves doctor id by linked user id or by matching doctor profile email to the login email; persists the link when resolved by email.</summary>
    int? GetDoctorIdForApplicationUser(string applicationUserId, string? userEmail);

    /// <summary>Doctors ordered by name for the SuperAdmin schedule picker dropdown.</summary>
    DoctorSchedulePickerViewModel GetDoctorSchedulePicker();
}
