using Microsoft.AspNetCore.Mvc.Rendering;
using SmileAnalysisBl.Helpers;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.AppointmentViewModels;
using SmileAnalysisBl.ViewModels.DoctorViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class AppointmentService : IAppointmentService
{
    private const int SlotStepMinutes = 30;

    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<AppointmentListViewModel> GetAll()
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var patients = _unitOfWork.GetRepository<Patient>().GetAll().ToDictionary(p => p.Id);
        var doctors = _unitOfWork.GetRepository<Doctor>().GetAll().ToDictionary(d => d.Id);
        var staff = _unitOfWork.GetRepository<Staff>().GetAll().ToDictionary(s => s.Id);

        return _unitOfWork.GetRepository<Appointment>()
            .GetAll()
            .OrderByDescending(a => a.AppointmentTime)
            .Select(a =>
            {
                patients.TryGetValue(a.PatientId, out var p);
                doctors.TryGetValue(a.DoctorId, out var d);
                var staffName = "—";
                if (a.StaffId is { } sid && staff.TryGetValue(sid, out var stMember))
                    staffName = $"{stMember.FirstName} {stMember.LastName}";
                return new AppointmentListViewModel
                {
                    Id = a.Id,
                    PatientName = p is null ? "—" : $"{p.FirstName} {p.LastName}",
                    DoctorName = d is null ? "—" : $"{d.FirstName} {d.LastName}",
                    StaffName = staffName,
                    AppointmentTime = a.AppointmentTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = a.Status
                };
            });
    }

    public DoctorSchedulePageViewModel? GetDoctorSchedule(int doctorId, DateOnly? rangeStart = null, DateOnly? rangeEnd = null)
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var doctor = _unitOfWork.GetRepository<Doctor>().GetById(doctorId);
        if (doctor is null)
            return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = rangeStart ?? today;
        var end = rangeEnd ?? start.AddDays(41);
        if (end < start)
            (start, end) = (end, start);
        if (end > start.AddDays(366))
            end = start.AddDays(366);

        var fromDt = start.ToDateTime(TimeOnly.MinValue);
        var toExclusive = end.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var patients = _unitOfWork.GetRepository<Patient>().GetAll().ToDictionary(p => p.Id);
        var doctors = _unitOfWork.GetRepository<Doctor>().GetAll().ToDictionary(d => d.Id);
        var staff = _unitOfWork.GetRepository<Staff>().GetAll().ToDictionary(s => s.Id);
        var prescriptionAppointmentIds = _unitOfWork.GetRepository<Prescription>().GetAll()
            .Select(p => p.AppointmentId)
            .ToHashSet();

        var list = _unitOfWork.GetRepository<Appointment>().GetAll()
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime >= fromDt && a.AppointmentTime < toExclusive)
            .OrderBy(a => a.AppointmentTime)
            .Select(a =>
            {
                patients.TryGetValue(a.PatientId, out var p);
                doctors.TryGetValue(a.DoctorId, out var d);
                var staffName = "—";
                if (a.StaffId is { } sid && staff.TryGetValue(sid, out var stMember))
                    staffName = $"{stMember.FirstName} {stMember.LastName}";
                return new AppointmentListViewModel
                {
                    Id = a.Id,
                    PatientName = p is null ? "—" : $"{p.FirstName} {p.LastName}",
                    DoctorName = d is null ? "—" : $"{d.FirstName} {d.LastName}",
                    StaffName = staffName,
                    AppointmentTime = a.AppointmentTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = a.Status,
                    HasPrescription = prescriptionAppointmentIds.Contains(a.Id)
                };
            })
            .ToList();

        return new DoctorSchedulePageViewModel
        {
            DoctorId = doctorId,
            DoctorDisplayName = $"Dr. {doctor.FirstName} {doctor.LastName}".Trim(),
            RangeStart = start,
            RangeEnd = end,
            Appointments = list
        };
    }

    public CreateAppointmentViewModel GetCreateForm(int? preselectPatientId = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var vm = new CreateAppointmentViewModel
        {
            AppointmentDate = today
        };

        var patientList = _unitOfWork.GetRepository<Patient>().GetAll()
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
        vm.Patients =
        [
            new SelectListItem("-- Select patient --", ""),
            .. patientList.Select(p => new SelectListItem($"{p.FirstName} {p.LastName} (P-{p.Id:0000})", p.Id.ToString()))
        ];

        var doctors = _unitOfWork.GetRepository<Doctor>().GetAll();
        vm.Doctors =
        [
            new SelectListItem("-- Select doctor --", ""),
            .. doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
                .Select(d => new SelectListItem($"{d.FirstName} {d.LastName}", d.Id.ToString()))
        ];

        if (preselectPatientId is > 0 && patientList.Any(p => p.Id == preselectPatientId))
            vm.PatientId = preselectPatientId;

        return vm;
    }

    public EditAppointmentViewModel? GetForEdit(int id)
    {
        var a = _unitOfWork.GetRepository<Appointment>().GetById(id);
        if (a is null || a.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            return null;

        var vm = new EditAppointmentViewModel
        {
            Id = a.Id,
            PatientId = a.PatientId,
            DoctorId = a.DoctorId,
            AppointmentDate = DateOnly.FromDateTime(a.AppointmentTime),
            AppointmentStartTime = TimeOnly.FromDateTime(a.AppointmentTime),
            DurationMinutes = a.DurationMinutes
        };
        PopulateEditSelectLists(vm);
        return vm;
    }

    private void PopulateEditSelectLists(EditAppointmentViewModel vm)
    {
        var patientList = _unitOfWork.GetRepository<Patient>().GetAll()
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
        vm.Patients =
        [
            new SelectListItem("-- Select patient --", ""),
            .. patientList.Select(p => new SelectListItem($"{p.FirstName} {p.LastName} (P-{p.Id:0000})", p.Id.ToString()))
        ];

        var doctors = _unitOfWork.GetRepository<Doctor>().GetAll();
        vm.Doctors =
        [
            new SelectListItem("-- Select doctor --", ""),
            .. doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
                .Select(d => new SelectListItem($"{d.FirstName} {d.LastName}", d.Id.ToString()))
        ];
    }

    public IReadOnlyList<TimeOnly> GetAvailableSlotStarts(int doctorId, DateOnly date, int durationMinutes,
        int? excludeAppointmentId = null)
    {
        if (durationMinutes < 5 || durationMinutes > 480)
            return [];

        if (!_unitOfWork.GetRepository<Doctor>().GetAll(d => d.Id == doctorId).Any())
            return [];

        var day = date.DayOfWeek;
        var windows = _unitOfWork.GetRepository<DoctorAvailability>().GetAll()
            .Where(a => a.DoctorId == doctorId && a.DayOfWeek == day)
            .OrderBy(a => a.StartTime)
            .ToList();

        if (windows.Count == 0)
            return [];

        var appointments = _unitOfWork.GetRepository<Appointment>().GetAll()
            .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Cancelled)
            .ToList();

        var slots = new List<TimeOnly>();
        foreach (var w in windows)
        {
            var t = w.StartTime;
            while (true)
            {
                var slotEnd = t.AddMinutes(durationMinutes);
                if (slotEnd > w.EndTime)
                    break;

                if (!ConflictsWithDoctorCalendar(appointments, date, t, durationMinutes, excludeAppointmentId))
                    slots.Add(t);

                try
                {
                    t = t.AddMinutes(SlotStepMinutes);
                }
                catch
                {
                    break;
                }
            }
        }

        return slots.Distinct().OrderBy(x => x).ToList();
    }

    private static bool ConflictsWithDoctorCalendar(
        List<Appointment> doctorAppointments,
        DateOnly date,
        TimeOnly slotStart,
        int durationMinutes,
        int? excludeAppointmentId = null)
    {
        var slotStartDt = date.ToDateTime(slotStart);
        var slotEndDt = slotStartDt.AddMinutes(durationMinutes);

        return doctorAppointments.Any(a =>
        {
            if (excludeAppointmentId is not null && a.Id == excludeAppointmentId)
                return false;
            if (DateOnly.FromDateTime(a.AppointmentTime) != date)
                return false;
            var aEnd = a.AppointmentTime.AddMinutes(a.DurationMinutes);
            return a.AppointmentTime < slotEndDt && aEnd > slotStartDt;
        });
    }

    public bool TryCreate(CreateAppointmentViewModel model, out string? errorMessage)
    {
        errorMessage = null;

        if (model.PatientId is null or <= 0)
        {
            errorMessage = "Please select a patient.";
            return false;
        }

        if (model.DoctorId is null)
        {
            errorMessage = "Please select a doctor.";
            return false;
        }

        if (model.AppointmentDate is null || model.AppointmentStartTime is null)
        {
            errorMessage = "Please choose a date and start time.";
            return false;
        }

        var doctorId = model.DoctorId.Value;
        var duration = model.DurationMinutes;

        var patient = _unitOfWork.GetRepository<Patient>().GetById(model.PatientId.Value);
        if (patient is null)
        {
            errorMessage = "Selected patient was not found.";
            return false;
        }

        if (!_unitOfWork.GetRepository<Doctor>().GetAll(d => d.Id == doctorId).Any())
        {
            errorMessage = "Doctor not found.";
            return false;
        }

        DateTime appointmentTime;
        try
        {
            appointmentTime = model.AppointmentDate.Value.ToDateTime(model.AppointmentStartTime.Value);
        }
        catch
        {
            errorMessage = "Invalid date or time.";
            return false;
        }

        if (appointmentTime < DateTime.Now)
        {
            errorMessage = "Appointment must be in the future.";
            return false;
        }

        if (!IsWithinDoctorAvailability(doctorId, appointmentTime, duration))
        {
            errorMessage = "That time is outside this doctor’s availability for the chosen day.";
            return false;
        }

        var appts = _unitOfWork.GetRepository<Appointment>().GetAll()
            .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Cancelled)
            .ToList();
        if (ConflictsWithDoctorCalendar(appts, DateOnly.FromDateTime(appointmentTime),
                TimeOnly.FromDateTime(appointmentTime), duration, null))
        {
            errorMessage = "The doctor already has another appointment that overlaps this time.";
            return false;
        }

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctorId,
            StaffId = null,
            AppointmentTime = appointmentTime,
            DurationMinutes = duration,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.GetRepository<Appointment>().Add(appointment);
        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryUpdate(EditAppointmentViewModel model, out string? errorMessage)
    {
        errorMessage = null;
        var repo = _unitOfWork.GetRepository<Appointment>();
        var a = repo.GetById(model.Id);
        if (a is null)
        {
            errorMessage = "Appointment not found.";
            return false;
        }

        if (a.Status == AppointmentStatus.Cancelled)
        {
            errorMessage = "Cancelled appointments cannot be edited.";
            return false;
        }

        if (a.Status == AppointmentStatus.Completed)
        {
            errorMessage = "Completed appointments cannot be edited.";
            return false;
        }

        if (model.PatientId is null or <= 0)
        {
            errorMessage = "Please select a patient.";
            return false;
        }

        if (model.DoctorId is null)
        {
            errorMessage = "Please select a doctor.";
            return false;
        }

        if (model.AppointmentDate is null || model.AppointmentStartTime is null)
        {
            errorMessage = "Please choose a date and start time.";
            return false;
        }

        var doctorId = model.DoctorId.Value;
        var duration = model.DurationMinutes;

        if (_unitOfWork.GetRepository<Patient>().GetById(model.PatientId.Value) is null)
        {
            errorMessage = "Selected patient was not found.";
            return false;
        }

        if (!_unitOfWork.GetRepository<Doctor>().GetAll(d => d.Id == doctorId).Any())
        {
            errorMessage = "Doctor not found.";
            return false;
        }

        DateTime appointmentTime;
        try
        {
            appointmentTime = model.AppointmentDate.Value.ToDateTime(model.AppointmentStartTime.Value);
        }
        catch
        {
            errorMessage = "Invalid date or time.";
            return false;
        }

        if (!IsWithinDoctorAvailability(doctorId, appointmentTime, duration))
        {
            errorMessage = "That time is outside this doctor’s availability for the chosen day.";
            return false;
        }

        var appts = _unitOfWork.GetRepository<Appointment>().GetAll()
            .Where(x => x.DoctorId == doctorId && x.Status != AppointmentStatus.Cancelled)
            .ToList();
        if (ConflictsWithDoctorCalendar(appts, DateOnly.FromDateTime(appointmentTime),
                TimeOnly.FromDateTime(appointmentTime), duration, model.Id))
        {
            errorMessage = "The doctor already has another appointment that overlaps this time.";
            return false;
        }

        a.PatientId = model.PatientId.Value;
        a.DoctorId = doctorId;
        a.AppointmentTime = appointmentTime;
        a.DurationMinutes = duration;
        a.UpdatedAt = DateTime.UtcNow;
        repo.Update(a);
        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryCancel(int id, out string? errorMessage)
    {
        errorMessage = null;
        var repo = _unitOfWork.GetRepository<Appointment>();
        var a = repo.GetById(id);
        if (a is null)
        {
            errorMessage = "Appointment not found.";
            return false;
        }

        if (a.Status == AppointmentStatus.Cancelled)
        {
            errorMessage = "This appointment is already cancelled.";
            return false;
        }

        if (a.Status == AppointmentStatus.Completed)
        {
            errorMessage = "Completed appointments cannot be cancelled.";
            return false;
        }

        a.Status = AppointmentStatus.Cancelled;
        a.UpdatedAt = DateTime.UtcNow;
        repo.Update(a);
        return _unitOfWork.SaveChanges() > 0;
    }

    private bool IsWithinDoctorAvailability(int doctorId, DateTime appointmentTime, int durationMinutes)
    {
        var day = appointmentTime.DayOfWeek;
        var start = TimeOnly.FromDateTime(appointmentTime);
        var end = start.AddMinutes(durationMinutes);

        var windows = _unitOfWork.GetRepository<DoctorAvailability>().GetAll()
            .Where(a => a.DoctorId == doctorId && a.DayOfWeek == day)
            .ToList();

        return windows.Any(w => start >= w.StartTime && end <= w.EndTime);
    }
}
