using System.Globalization;
using SmileAnalysisBl.Helpers;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PatientViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGummySmileCaseService _gummySmileCaseService;
    private readonly IXRayAnalysisCaseService _xrayAnalysisCaseService;

    public PatientService(
        IUnitOfWork unitOfWork,
        IGummySmileCaseService gummySmileCaseService,
        IXRayAnalysisCaseService xrayAnalysisCaseService)
    {
        _unitOfWork = unitOfWork;
        _gummySmileCaseService = gummySmileCaseService;
        _xrayAnalysisCaseService = xrayAnalysisCaseService;
    }

    public IEnumerable<PatientListViewModel> GetAll()
    {
        return _unitOfWork.GetRepository<Patient>()
            .GetAll()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientListViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Gender = p.Gender,
                DateOfBirth = p.DateOfBirth,
                Phone = p.Phone,
                Email = p.Email
            });
    }

    public PatientRecordsPageViewModel GetPatientRecordsPage(string? search = null)
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var allPatients = _unitOfWork.GetRepository<Patient>().GetAll().ToList();
        var appointments = _unitOfWork.GetRepository<Appointment>().GetAll().ToList();
        var payments = _unitOfWork.GetRepository<Payment>().GetAll().ToList();

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var nextMonthStart = monthStart.AddMonths(1);

        var displayedPatients = allPatients;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            displayedPatients = allPatients
                .Where(p =>
                    (p.FirstName + " " + p.LastName).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Phone ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.Id.ToString(CultureInfo.InvariantCulture).Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var scheduledToday = appointments.Count(a => a.AppointmentTime.Date == today);
        var upcomingAppointments = appointments.Count(a =>
            a.Status == AppointmentStatus.Scheduled &&
            a.AppointmentTime.Date >= today);
        var monthlyRevenue = payments
            .Where(p => p.Status == PaymentStatus.Paid && p.PaymentDate >= monthStart && p.PaymentDate < nextMonthStart)
            .Sum(p => p.Amount);

        var prevMonthPatients = allPatients.Count(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < monthStart);
        var thisMonthPatients = allPatients.Count(p => p.CreatedAt >= monthStart);
        var growthPercent = prevMonthPatients == 0
            ? (thisMonthPatients > 0 ? 100 : 0)
            : (int)Math.Round((thisMonthPatients - prevMonthPatients) * 100m / prevMonthPatients);

        var rows = displayedPatients
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapDirectoryRow(p, appointments.Where(a => a.PatientId == p.Id), today))
            .ToList();

        return new PatientRecordsPageViewModel
        {
            SearchQuery = search,
            TotalPatients = allPatients.Count,
            MatchingCount = displayedPatients.Count,
            ScheduledToday = scheduledToday,
            UpcomingAppointments = upcomingAppointments,
            MonthlyRevenue = monthlyRevenue,
            PatientGrowthPercent = growthPercent,
            Rows = rows,
            IsDoctorMyPatientsView = false
        };
    }

    public PatientRecordsPageViewModel GetPatientRecordsPageForDoctor(int doctorId, string? search = null)
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var allPatients = _unitOfWork.GetRepository<Patient>().GetAll().ToList();
        var appointments = _unitOfWork.GetRepository<Appointment>().GetAll().ToList();
        var patientIdsWithDoctor = appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .ToHashSet();

        var myPatients = allPatients.Where(p => patientIdsWithDoctor.Contains(p.Id)).ToList();

        var displayedPatients = myPatients;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            displayedPatients = myPatients
                .Where(p =>
                    (p.FirstName + " " + p.LastName).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Phone ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.Id.ToString(CultureInfo.InvariantCulture).Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var nextMonthStart = monthStart.AddMonths(1);

        var scheduledToday = appointments.Count(a =>
            a.DoctorId == doctorId &&
            a.AppointmentTime.Date == today &&
            a.Status != AppointmentStatus.Cancelled);

        var upcomingAppointments = appointments.Count(a =>
            a.DoctorId == doctorId &&
            a.Status == AppointmentStatus.Scheduled &&
            a.AppointmentTime.Date >= today &&
            patientIdsWithDoctor.Contains(a.PatientId));

        var payments = _unitOfWork.GetRepository<Payment>().GetAll().ToList();
        var monthlyRevenue = payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaymentDate >= monthStart &&
                p.PaymentDate < nextMonthStart &&
                patientIdsWithDoctor.Contains(p.PatientId))
            .Sum(p => p.Amount);

        var prevMonthPatients = myPatients.Count(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < monthStart);
        var thisMonthPatients = myPatients.Count(p => p.CreatedAt >= monthStart);
        var growthPercent = prevMonthPatients == 0
            ? (thisMonthPatients > 0 ? 100 : 0)
            : (int)Math.Round((thisMonthPatients - prevMonthPatients) * 100m / prevMonthPatients);

        var docAppts = appointments.Where(a => a.DoctorId == doctorId).ToList();

        var rows = displayedPatients
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapDirectoryRow(p, docAppts.Where(a => a.PatientId == p.Id), today))
            .ToList();

        return new PatientRecordsPageViewModel
        {
            SearchQuery = search,
            TotalPatients = myPatients.Count,
            MatchingCount = displayedPatients.Count,
            ScheduledToday = scheduledToday,
            UpcomingAppointments = upcomingAppointments,
            MonthlyRevenue = monthlyRevenue,
            PatientGrowthPercent = growthPercent,
            Rows = rows,
            IsDoctorMyPatientsView = true
        };
    }

    public bool PatientHasAppointmentWithDoctor(int patientId, int doctorId) =>
        _unitOfWork.GetRepository<Appointment>().GetAll()
            .Any(a => a.PatientId == patientId && a.DoctorId == doctorId);

    private static PatientDirectoryRowViewModel MapDirectoryRow(Patient p, IEnumerable<Appointment> appointmentsForRow, DateTime today)
    {
        var mine = appointmentsForRow.ToList();
        var lastVisit = mine
            .Where(a => a.AppointmentTime.Date <= today)
            .OrderByDescending(a => a.AppointmentTime)
            .FirstOrDefault();
        var nextAppt = mine
            .Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentTime.Date >= today)
            .OrderBy(a => a.AppointmentTime)
            .FirstOrDefault();

        var (nextLabel, badgeClass) = NextAppointmentDisplay(nextAppt, today);

        var contact = !string.IsNullOrWhiteSpace(p.Phone)
            ? p.Phone
            : (!string.IsNullOrWhiteSpace(p.Email) ? p.Email : "—");

        return new PatientDirectoryRowViewModel
        {
            Id = p.Id,
            DisplayId = $"P-{p.Id:0000}",
            FullName = $"{p.FirstName} {p.LastName}".Trim(),
            Initials = Initials(p.FirstName, p.LastName),
            AvatarClass = AvatarClassFor(p.Id),
            ContactDisplay = contact,
            LastVisitDisplay = lastVisit is null ? "—" : lastVisit.AppointmentTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture),
            NextAppointmentLabel = nextLabel,
            NextAppointmentBadgeClass = badgeClass
        };
    }

    private static (string Label, string BadgeClass) NextAppointmentDisplay(Appointment? next, DateTime today)
    {
        if (next is null)
            return ("Not Scheduled", "cc-badge-muted");

        var d = next.AppointmentTime.Date;
        if (d == today.AddDays(1))
            return ("Tomorrow", "cc-badge-emerald");

        if (d == today)
            return ("Today", "cc-badge-blue");

        return (next.AppointmentTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture), "cc-badge-blue");
    }

    private static string Initials(string first, string last)
    {
        var a = string.IsNullOrWhiteSpace(first) ? '?' : char.ToUpperInvariant(first[0]);
        var b = string.IsNullOrWhiteSpace(last) ? "" : char.ToUpperInvariant(last[0]).ToString();
        return $"{a}{b}";
    }

    private static string AvatarClassFor(int id) =>
        (Math.Abs(id) % 4) switch
        {
            0 => "cc-avatar-blue",
            1 => "cc-avatar-slate",
            2 => "cc-avatar-amber",
            _ => "cc-avatar-purple"
        };

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }

    public bool Create(CreatePatientViewModel model)
    {
        var patient = new Patient
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Gender = model.Gender,
            DateOfBirth = model.DateOfBirth.Date,
            Phone = model.Phone,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow,
            ClinicalHistory = HasClinicalContent(model.Clinical) ? MapToNewClinicalEntity(model.Clinical) : null
        };
        _unitOfWork.GetRepository<Patient>().Add(patient);
        return _unitOfWork.SaveChanges() > 0;
    }

    public PatientDetailsPageViewModel? GetPatientDetails(int id)
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var p = _unitOfWork.GetRepository<Patient>().GetById(id);
        if (p is null) return null;

        var appointments = _unitOfWork.GetRepository<Appointment>()
            .GetAll()
            .Where(a => a.PatientId == id)
            .ToList();

        var today = DateTime.Today;
        var lastVisit = appointments
            .Where(a => a.AppointmentTime.Date <= today)
            .OrderByDescending(a => a.AppointmentTime)
            .FirstOrDefault();
        var nextAppt = appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentTime.Date >= today)
            .OrderBy(a => a.AppointmentTime)
            .FirstOrDefault();

        var (nextLabel, _) = NextAppointmentDisplay(nextAppt, today);
        var gummyAnalyses = _gummySmileCaseService.GetPatientAnalyses(id);
        var xrayAnalyses = _xrayAnalysisCaseService.GetPatientAnalyses(id);

        var doctors = _unitOfWork.GetRepository<Doctor>().GetAll().ToDictionary(d => d.Id);
        var prescriptions = _unitOfWork.GetRepository<Prescription>().GetAll().ToList();
        var prescriptionByAppointment = prescriptions.ToDictionary(p => p.AppointmentId);
        var prescriptionItems = _unitOfWork.GetRepository<PrescriptionItem>().GetAll().ToList();
        var itemsByPrescription = prescriptionItems.GroupBy(i => i.PrescriptionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var appointmentHistory = appointments
            .OrderByDescending(a => a.AppointmentTime)
            .Select(a =>
            {
                doctors.TryGetValue(a.DoctorId, out var doc);
                prescriptionByAppointment.TryGetValue(a.Id, out var rx);
                return new PatientAppointmentHistoryViewModel
                {
                    Id = a.Id,
                    AppointmentTime = a.AppointmentTime,
                    DateDisplay = a.AppointmentTime.ToString("MMM dd, yyyy · h:mm tt", CultureInfo.InvariantCulture),
                    DoctorName = doc is null ? "—" : $"Dr. {doc.FirstName} {doc.LastName}".Trim(),
                    Status = a.Status,
                    StatusBadgeClass = a.Status switch
                    {
                        AppointmentStatus.Scheduled => "cc-badge-blue",
                        AppointmentStatus.Completed => "cc-badge-emerald",
                        AppointmentStatus.Cancelled => "cc-badge-muted",
                        _ => "cc-badge-muted"
                    },
                    HasPrescription = rx is not null,
                    PrescriptionId = rx?.Id
                };
            })
            .ToList();

        var prescriptionHistory = prescriptions
            .Where(p => appointments.Any(a => a.Id == p.AppointmentId))
            .Select(p =>
            {
                var appt = appointments.First(a => a.Id == p.AppointmentId);
                doctors.TryGetValue(appt.DoctorId, out var doc);
                itemsByPrescription.TryGetValue(p.Id, out var items);
                items ??= [];
                var names = items.Select(i => i.MedicationName).Take(3).ToList();
                var summary = names.Count == 0
                    ? "No medications"
                    : string.Join(", ", names) + (items.Count > 3 ? $" +{items.Count - 3} more" : "");
                return new PatientPrescriptionHistoryViewModel
                {
                    Id = p.Id,
                    AppointmentId = p.AppointmentId,
                    AppointmentDate = appt.AppointmentTime,
                    DateDisplay = appt.AppointmentTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture),
                    DoctorName = doc is null ? "—" : $"Dr. {doc.FirstName} {doc.LastName}".Trim(),
                    MedicationCount = items.Count,
                    MedicationSummary = summary,
                    NotesPreview = string.IsNullOrWhiteSpace(p.Notes)
                        ? null
                        : (p.Notes.Length > 80 ? p.Notes[..80] + "…" : p.Notes)
                };
            })
            .OrderByDescending(p => p.AppointmentDate)
            .ToList();

        var clinical = _unitOfWork.GetRepository<PatientClinicalHistory>().GetAll()
            .FirstOrDefault(h => h.PatientId == id);
        var clinicalNotes = MapClinicalNotes(clinical);

        return new PatientDetailsPageViewModel
        {
            Id = p.Id,
            DisplayId = $"P-{p.Id:0000}",
            FullName = $"{p.FirstName} {p.LastName}".Trim(),
            Initials = Initials(p.FirstName, p.LastName),
            AvatarClass = AvatarClassFor(p.Id),
            Age = CalculateAge(p.DateOfBirth),
            GenderDisplay = p.Gender.ToString(),
            Phone = p.Phone,
            Email = p.Email,
            LastVisitDisplay = lastVisit is null
                ? "—"
                : lastVisit.AppointmentTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture),
            NextAppointmentDisplay = nextLabel,
            TotalAppointments = appointments.Count,
            CompletedAnalyses = gummyAnalyses.Count + xrayAnalyses.Count,
            GummySmileCount = gummyAnalyses.Count,
            XRayCount = xrayAnalyses.Count,
            GummySmileAnalyses = gummyAnalyses,
            XRayAnalyses = xrayAnalyses,
            AppointmentHistory = appointmentHistory,
            PrescriptionHistory = prescriptionHistory,
            ClinicalNotes = clinicalNotes
        };
    }

    private static PatientClinicalNotesViewModel MapClinicalNotes(PatientClinicalHistory? h)
    {
        if (h is null)
            return new PatientClinicalNotesViewModel();

        var hasNotes = !string.IsNullOrWhiteSpace(h.ChronicDiseases)
                       || !string.IsNullOrWhiteSpace(h.CurrentMedications)
                       || !string.IsNullOrWhiteSpace(h.Allergies)
                       || !string.IsNullOrWhiteSpace(h.PreviousSurgeries)
                       || !string.IsNullOrWhiteSpace(h.HeartOrBleedingConditions)
                       || !string.IsNullOrWhiteSpace(h.CurrentPainDetails)
                       || !string.IsNullOrWhiteSpace(h.PreviousDentalTreatments)
                       || !string.IsNullOrWhiteSpace(h.GumProblems)
                       || !string.IsNullOrWhiteSpace(h.OralHabits)
                       || h.LastDentalVisit.HasValue;

        return new PatientClinicalNotesViewModel
        {
            HasNotes = hasNotes,
            ChronicDiseases = h.ChronicDiseases,
            CurrentMedications = h.CurrentMedications,
            Allergies = h.Allergies,
            PreviousSurgeries = h.PreviousSurgeries,
            HeartOrBleedingConditions = h.HeartOrBleedingConditions,
            CurrentPainDetails = h.CurrentPainDetails,
            PreviousDentalTreatments = h.PreviousDentalTreatments,
            GumProblems = h.GumProblems,
            OralHabits = h.OralHabits,
            LastDentalVisitDisplay = h.LastDentalVisit?.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture)
        };
    }

    public GummySmileAnalysisViewModel? GetGummySmileAnalysisPage(int patientId)
    {
        var p = _unitOfWork.GetRepository<Patient>().GetById(patientId);
        if (p is null) return null;

        return new GummySmileAnalysisViewModel
        {
            PatientId = p.Id,
            PatientName = $"{p.FirstName} {p.LastName}".Trim(),
            DisplayId = $"P-{p.Id:0000}"
        };
    }

    public EditPatientViewModel? GetForEdit(int id)
    {
        var p = _unitOfWork.GetRepository<Patient>().GetById(id);
        if (p is null) return null;
        var history = _unitOfWork.GetRepository<PatientClinicalHistory>().GetAll()
            .FirstOrDefault(h => h.PatientId == id);
        return new EditPatientViewModel
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth,
            Phone = p.Phone,
            Email = p.Email,
            Clinical = MapClinicalToInput(history)
        };
    }

    public bool Update(EditPatientViewModel model)
    {
        var repo = _unitOfWork.GetRepository<Patient>();
        var p = repo.GetById(model.Id);
        if (p is null) return false;
        p.FirstName = model.FirstName;
        p.LastName = model.LastName;
        p.Gender = model.Gender;
        p.DateOfBirth = model.DateOfBirth.Date;
        p.Phone = model.Phone;
        p.Email = model.Email;
        p.UpdatedAt = DateTime.UtcNow;
        repo.Update(p);
        UpsertClinicalHistory(model.Id, model.Clinical);
        return _unitOfWork.SaveChanges() > 0;
    }

    private static PatientClinicalHistory MapToNewClinicalEntity(PatientClinicalHistoryInputModel input)
    {
        var utc = DateTime.UtcNow;
        return new PatientClinicalHistory
        {
            CreatedAt = utc,
            ChronicDiseases = NullIfWhiteSpace(input.ChronicDiseases),
            CurrentMedications = NullIfWhiteSpace(input.CurrentMedications),
            Allergies = NullIfWhiteSpace(input.Allergies),
            PreviousSurgeries = NullIfWhiteSpace(input.PreviousSurgeries),
            HeartOrBleedingConditions = NullIfWhiteSpace(input.HeartOrBleedingConditions),
            LastDentalVisit = input.LastDentalVisit?.Date,
            CurrentPainDetails = NullIfWhiteSpace(input.CurrentPainDetails),
            PreviousDentalTreatments = NullIfWhiteSpace(input.PreviousDentalTreatments),
            GumProblems = NullIfWhiteSpace(input.GumProblems),
            OralHabits = NullIfWhiteSpace(input.OralHabits)
        };
    }

    private static PatientClinicalHistoryInputModel MapClinicalToInput(PatientClinicalHistory? h)
    {
        if (h is null)
            return new PatientClinicalHistoryInputModel();
        return new PatientClinicalHistoryInputModel
        {
            ChronicDiseases = h.ChronicDiseases,
            CurrentMedications = h.CurrentMedications,
            Allergies = h.Allergies,
            PreviousSurgeries = h.PreviousSurgeries,
            HeartOrBleedingConditions = h.HeartOrBleedingConditions,
            LastDentalVisit = h.LastDentalVisit,
            CurrentPainDetails = h.CurrentPainDetails,
            PreviousDentalTreatments = h.PreviousDentalTreatments,
            GumProblems = h.GumProblems,
            OralHabits = h.OralHabits
        };
    }

    private void UpsertClinicalHistory(int patientId, PatientClinicalHistoryInputModel input)
    {
        var histRepo = _unitOfWork.GetRepository<PatientClinicalHistory>();
        var existing = histRepo.GetAll().FirstOrDefault(x => x.PatientId == patientId);
        if (existing is null)
        {
            if (!HasClinicalContent(input))
                return;
            var entity = MapToNewClinicalEntity(input);
            entity.PatientId = patientId;
            histRepo.Add(entity);
            return;
        }

        existing.ChronicDiseases = NullIfWhiteSpace(input.ChronicDiseases);
        existing.CurrentMedications = NullIfWhiteSpace(input.CurrentMedications);
        existing.Allergies = NullIfWhiteSpace(input.Allergies);
        existing.PreviousSurgeries = NullIfWhiteSpace(input.PreviousSurgeries);
        existing.HeartOrBleedingConditions = NullIfWhiteSpace(input.HeartOrBleedingConditions);
        existing.LastDentalVisit = input.LastDentalVisit?.Date;
        existing.CurrentPainDetails = NullIfWhiteSpace(input.CurrentPainDetails);
        existing.PreviousDentalTreatments = NullIfWhiteSpace(input.PreviousDentalTreatments);
        existing.GumProblems = NullIfWhiteSpace(input.GumProblems);
        existing.OralHabits = NullIfWhiteSpace(input.OralHabits);
        existing.UpdatedAt = DateTime.UtcNow;
        histRepo.Update(existing);
    }

    private static string? NullIfWhiteSpace(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool HasClinicalContent(PatientClinicalHistoryInputModel input) =>
        input.LastDentalVisit.HasValue
        || !string.IsNullOrWhiteSpace(input.ChronicDiseases)
        || !string.IsNullOrWhiteSpace(input.CurrentMedications)
        || !string.IsNullOrWhiteSpace(input.Allergies)
        || !string.IsNullOrWhiteSpace(input.PreviousSurgeries)
        || !string.IsNullOrWhiteSpace(input.HeartOrBleedingConditions)
        || !string.IsNullOrWhiteSpace(input.CurrentPainDetails)
        || !string.IsNullOrWhiteSpace(input.PreviousDentalTreatments)
        || !string.IsNullOrWhiteSpace(input.GumProblems)
        || !string.IsNullOrWhiteSpace(input.OralHabits);

    public bool TryDelete(int id, out string? error)
    {
        error = null;
        var appts = _unitOfWork.GetRepository<Appointment>().GetAll();
        if (appts.Any(a => a.PatientId == id))
        {
            error = "Remove or reassign this patient’s appointments before deleting.";
            return false;
        }

        var payments = _unitOfWork.GetRepository<Payment>().GetAll();
        if (payments.Any(x => x.PatientId == id))
        {
            error = "Remove payment records for this patient before deleting.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<Patient>();
        var p = repo.GetById(id);
        if (p is null)
        {
            error = "Patient not found.";
            return false;
        }

        try
        {
            repo.Delete(p);
            return _unitOfWork.SaveChanges() > 0;
        }
        catch
        {
            error = "Could not delete this patient.";
            return false;
        }
    }
}
