using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PrescriptionViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public PrescriptionDetailsViewModel? GetByAppointmentId(int appointmentId)
    {
        var prescription = FindByAppointmentId(appointmentId);
        if (prescription is null)
            return null;

        return MapDetails(prescription);
    }

    public PrescriptionDetailsViewModel? GetById(int prescriptionId)
    {
        var prescription = _unitOfWork.GetRepository<Prescription>().GetById(prescriptionId);
        return prescription is null ? null : MapDetails(prescription);
    }

    public PrescriptionFormViewModel? GetCreateForm(int appointmentId)
    {
        if (HasPrescriptionForAppointment(appointmentId))
            return null;

        return BuildForm(appointmentId, isEdit: false);
    }

    public PrescriptionFormViewModel? GetEditForm(int appointmentId)
    {
        var prescription = FindByAppointmentId(appointmentId);
        if (prescription is null)
            return null;

        var form = BuildForm(appointmentId, isEdit: true);
        if (form is null)
            return null;

        form.PrescriptionId = prescription.Id;
        form.Notes = prescription.Notes;
        form.Items = GetItems(prescription.Id)
            .Select(i => new PrescriptionItemInputViewModel
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Duration = i.Duration,
                Instructions = i.Instructions
            })
            .ToList();

        if (form.Items.Count == 0)
            form.Items.Add(new PrescriptionItemInputViewModel());

        return form;
    }

    public bool TryCreate(PrescriptionFormViewModel model, int? doctorId, bool isSuperAdmin, out string? errorMessage)
    {
        errorMessage = null;

        if (!ValidateAccess(model.AppointmentId, doctorId, isSuperAdmin, out errorMessage))
            return false;

        if (HasPrescriptionForAppointment(model.AppointmentId))
        {
            errorMessage = "A prescription already exists for this appointment.";
            return false;
        }

        if (!ValidateItems(model.Items, out errorMessage))
            return false;

        var prescription = new Prescription
        {
            AppointmentId = model.AppointmentId,
            Notes = Normalize(model.Notes),
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.GetRepository<Prescription>().Add(prescription);
        if (_unitOfWork.SaveChanges() <= 0)
        {
            errorMessage = "Could not save prescription.";
            return false;
        }

        AddItems(prescription.Id, model.Items);
        if (_unitOfWork.SaveChanges() <= 0)
        {
            errorMessage = "Prescription was created but medications could not be saved.";
            return false;
        }

        return true;
    }

    public bool TryUpdate(PrescriptionFormViewModel model, int? doctorId, bool isSuperAdmin, out string? errorMessage)
    {
        errorMessage = null;

        if (model.PrescriptionId is not { } prescriptionId || prescriptionId <= 0)
        {
            errorMessage = "Prescription id is required for update.";
            return false;
        }

        var prescription = _unitOfWork.GetRepository<Prescription>().GetById(prescriptionId);
        if (prescription is null)
        {
            errorMessage = "Prescription not found.";
            return false;
        }

        if (prescription.AppointmentId != model.AppointmentId)
        {
            errorMessage = "Appointment mismatch.";
            return false;
        }

        if (!ValidateAccess(model.AppointmentId, doctorId, isSuperAdmin, out errorMessage))
            return false;

        if (!ValidateItems(model.Items, out errorMessage))
            return false;

        prescription.Notes = Normalize(model.Notes);
        prescription.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.GetRepository<Prescription>().Update(prescription);

        SyncItems(prescriptionId, model.Items);

        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryDelete(int prescriptionId, int? doctorId, bool isSuperAdmin, out string? errorMessage)
    {
        errorMessage = null;

        var prescription = _unitOfWork.GetRepository<Prescription>().GetById(prescriptionId);
        if (prescription is null)
        {
            errorMessage = "Prescription not found.";
            return false;
        }

        if (!ValidateAccess(prescription.AppointmentId, doctorId, isSuperAdmin, out errorMessage))
            return false;

        var itemRepo = _unitOfWork.GetRepository<PrescriptionItem>();
        foreach (var item in GetItems(prescriptionId))
            itemRepo.Delete(item);

        _unitOfWork.GetRepository<Prescription>().Delete(prescription);
        return _unitOfWork.SaveChanges() > 0;
    }

    public bool DoctorCanManageAppointment(int? doctorId, int appointmentId)
    {
        if (doctorId is null or <= 0)
            return false;

        var appointment = _unitOfWork.GetRepository<Appointment>().GetById(appointmentId);
        return appointment?.DoctorId == doctorId;
    }

    public bool AppointmentExists(int appointmentId) =>
        _unitOfWork.GetRepository<Appointment>().GetById(appointmentId) is not null;

    public bool HasPrescriptionForAppointment(int appointmentId) =>
        FindByAppointmentId(appointmentId) is not null;

    private Prescription? FindByAppointmentId(int appointmentId) =>
        _unitOfWork.GetRepository<Prescription>()
            .GetAll(p => p.AppointmentId == appointmentId)
            .FirstOrDefault();

    private List<PrescriptionItem> GetItems(int prescriptionId) =>
        _unitOfWork.GetRepository<PrescriptionItem>()
            .GetAll(i => i.PrescriptionId == prescriptionId)
            .OrderBy(i => i.Id)
            .ToList();

    private PrescriptionFormViewModel? BuildForm(int appointmentId, bool isEdit)
    {
        var appointment = _unitOfWork.GetRepository<Appointment>().GetById(appointmentId);
        if (appointment is null)
            return null;

        var patient = _unitOfWork.GetRepository<Patient>().GetById(appointment.PatientId);
        var doctor = _unitOfWork.GetRepository<Doctor>().GetById(appointment.DoctorId);
        if (patient is null || doctor is null)
            return null;

        return new PrescriptionFormViewModel
        {
            AppointmentId = appointmentId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            PatientDisplayId = $"P-{patient.Id:0000}",
            DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}".Trim(),
            AppointmentDate = appointment.AppointmentTime,
            AppointmentStatus = appointment.Status.ToString(),
            IsEdit = isEdit,
            Items = [new PrescriptionItemInputViewModel()]
        };
    }

    private PrescriptionDetailsViewModel MapDetails(Prescription prescription)
    {
        var appointment = _unitOfWork.GetRepository<Appointment>().GetById(prescription.AppointmentId)!;
        var patient = _unitOfWork.GetRepository<Patient>().GetById(appointment.PatientId)!;
        var doctor = _unitOfWork.GetRepository<Doctor>().GetById(appointment.DoctorId)!;

        return new PrescriptionDetailsViewModel
        {
            Id = prescription.Id,
            AppointmentId = prescription.AppointmentId,
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            PatientDisplayId = $"P-{patient.Id:0000}",
            DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}".Trim(),
            AppointmentDate = appointment.AppointmentTime,
            Notes = prescription.Notes,
            CreatedAt = prescription.CreatedAt,
            Items = GetItems(prescription.Id).Select(i => new PrescriptionItemDetailsViewModel
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Duration = i.Duration,
                Instructions = i.Instructions
            }).ToList()
        };
    }

    private bool ValidateAccess(int appointmentId, int? doctorId, bool isSuperAdmin, out string? errorMessage)
    {
        errorMessage = null;

        if (!AppointmentExists(appointmentId))
        {
            errorMessage = "Appointment not found.";
            return false;
        }

        if (isSuperAdmin)
            return true;

        if (!DoctorCanManageAppointment(doctorId, appointmentId))
        {
            errorMessage = "Only the assigned doctor can manage prescriptions for this appointment.";
            return false;
        }

        return true;
    }

    private static bool ValidateItems(IReadOnlyList<PrescriptionItemInputViewModel> items, out string? errorMessage)
    {
        errorMessage = null;

        var validItems = items
            .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName)
                        || !string.IsNullOrWhiteSpace(i.Dosage)
                        || !string.IsNullOrWhiteSpace(i.Duration))
            .ToList();

        if (validItems.Count == 0)
        {
            errorMessage = "Add at least one medication.";
            return false;
        }

        foreach (var item in validItems)
        {
            if (!ValidateItem(item.MedicationName, item.Dosage, item.Duration, out errorMessage))
                return false;
        }

        return true;
    }

    private static bool ValidateItem(string medicationName, string dosage, string duration, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            errorMessage = "Medication name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dosage))
        {
            errorMessage = "Dosage is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(duration))
        {
            errorMessage = "Duration is required.";
            return false;
        }

        if (medicationName.Length > 200 || dosage.Length > 100 || duration.Length > 100)
        {
            errorMessage = "One or more medication fields exceed the maximum length.";
            return false;
        }

        return true;
    }

    private void AddItems(int prescriptionId, IEnumerable<PrescriptionItemInputViewModel> items)
    {
        var repo = _unitOfWork.GetRepository<PrescriptionItem>();
        foreach (var item in items.Where(i => !string.IsNullOrWhiteSpace(i.MedicationName)))
        {
            repo.Add(new PrescriptionItem
            {
                PrescriptionId = prescriptionId,
                MedicationName = item.MedicationName.Trim(),
                Dosage = item.Dosage.Trim(),
                Duration = item.Duration.Trim(),
                Instructions = Normalize(item.Instructions),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private void SyncItems(int prescriptionId, IEnumerable<PrescriptionItemInputViewModel> items)
    {
        var repo = _unitOfWork.GetRepository<PrescriptionItem>();
        var existing = GetItems(prescriptionId).ToDictionary(i => i.Id);
        var incoming = items
            .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName))
            .ToList();

        var incomingIds = incoming.Where(i => i.Id is > 0).Select(i => i.Id!.Value).ToHashSet();

        foreach (var old in existing.Values.Where(e => !incomingIds.Contains(e.Id)))
            repo.Delete(old);

        foreach (var item in incoming)
        {
            if (item.Id is > 0 && existing.TryGetValue(item.Id.Value, out var entity))
            {
                entity.MedicationName = item.MedicationName.Trim();
                entity.Dosage = item.Dosage.Trim();
                entity.Duration = item.Duration.Trim();
                entity.Instructions = Normalize(item.Instructions);
                entity.UpdatedAt = DateTime.UtcNow;
                repo.Update(entity);
            }
            else
            {
                repo.Add(new PrescriptionItem
                {
                    PrescriptionId = prescriptionId,
                    MedicationName = item.MedicationName.Trim(),
                    Dosage = item.Dosage.Trim(),
                    Duration = item.Duration.Trim(),
                    Instructions = Normalize(item.Instructions),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
