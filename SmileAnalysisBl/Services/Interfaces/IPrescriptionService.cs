using SmileAnalysisBl.ViewModels.PrescriptionViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IPrescriptionService
{
    PrescriptionDetailsViewModel? GetByAppointmentId(int appointmentId);
    PrescriptionDetailsViewModel? GetById(int prescriptionId);
    PrescriptionFormViewModel? GetCreateForm(int appointmentId);
    PrescriptionFormViewModel? GetEditForm(int appointmentId);

    bool TryCreate(PrescriptionFormViewModel model, int? doctorId, bool isSuperAdmin, out string? errorMessage);
    bool TryUpdate(PrescriptionFormViewModel model, int? doctorId, bool isSuperAdmin, out string? errorMessage);
    bool TryDelete(int prescriptionId, int? doctorId, bool isSuperAdmin, out string? errorMessage);

    bool DoctorCanManageAppointment(int? doctorId, int appointmentId);
    bool AppointmentExists(int appointmentId);
    bool HasPrescriptionForAppointment(int appointmentId);
}
