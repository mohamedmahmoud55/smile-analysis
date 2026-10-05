using SmileAnalysisBl.ViewModels.PatientViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IPatientService
{
    IEnumerable<PatientListViewModel> GetAll();
    PatientRecordsPageViewModel GetPatientRecordsPage(string? search = null);

    /// <summary>Patients who have at least one appointment with <paramref name="doctorId"/>; stats scoped to that doctor / those patients.</summary>
    PatientRecordsPageViewModel GetPatientRecordsPageForDoctor(int doctorId, string? search = null);

    /// <summary>True if the patient has any appointment (including cancelled) with this doctor.</summary>
    bool PatientHasAppointmentWithDoctor(int patientId, int doctorId);

    bool Create(CreatePatientViewModel model);
    EditPatientViewModel? GetForEdit(int id);
    PatientDetailsPageViewModel? GetPatientDetails(int id);
    GummySmileAnalysisViewModel? GetGummySmileAnalysisPage(int patientId);
    bool Update(EditPatientViewModel model);
    /// <summary>Returns false with <paramref name="error"/> when the patient cannot be removed (e.g. linked records).</summary>
    bool TryDelete(int id, out string? error);
}
