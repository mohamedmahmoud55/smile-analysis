using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.DoctorViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public DoctorRecordsPageViewModel GetMedicalTeamPage(string? search = null)
    {
        var now = DateTime.Now;
        var allAvail = _unitOfWork.GetRepository<DoctorAvailability>().GetAll().ToList();
        var availByDoctor = allAvail.GroupBy(a => a.DoctorId).ToDictionary(g => g.Key, g => g.ToList());

        var allDoctors = _unitOfWork.GetRepository<Doctor>().GetAll().OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();
        var staff = _unitOfWork.GetRepository<Staff>().GetAll().ToList();
        var patientCount = _unitOfWork.GetRepository<Patient>().GetAll().Count();

        var displayedDoctors = allDoctors;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            displayedDoctors = allDoctors
                .Where(d =>
                    (d.FirstName + " " + d.LastName).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    d.Specialty.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    d.Id.ToString(CultureInfo.InvariantCulture).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(d.Email) && d.Email.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.Phone) && d.Phone.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var doctorCount = Math.Max(allDoctors.Count, 1);
        var ratio = patientCount / (decimal)doctorCount;
        var ratioDisplay = $"1:{ratio:0.0}";

        var rows = displayedDoctors
            .Select(d => MapRow(d, availByDoctor.GetValueOrDefault(d.Id) ?? [], now))
            .ToList();

        return new DoctorRecordsPageViewModel
        {
            SearchQuery = search,
            TotalStaffCount = staff.Count + allDoctors.Count,
            MatchingCount = displayedDoctors.Count,
            TotalDoctors = allDoctors.Count,
            OnDutyApprox = allDoctors.Count == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(allDoctors.Count * 0.42)),
            PatientRatioDisplay = ratioDisplay,
            SatisfactionPercent = 98,
            Rows = rows
        };
    }

    private static DoctorDirectoryRowViewModel MapRow(Doctor d, List<DoctorAvailability> slots, DateTime now)
    {
        var (avail, badge) = MapAvailabilityBadge(slots, now);
        var (birthDisplay, ageDisplay) = FormatBirthAndAge(d.BirthDate);

        return new DoctorDirectoryRowViewModel
        {
            Id = d.Id,
            DisplayId = $"D-{d.Id:0000}",
            FullName = $"Dr. {d.FirstName} {d.LastName}".Trim(),
            Initials = Initials(d.FirstName, d.LastName),
            AvatarClass = AvatarClassFor(d.Id),
            Specialty = d.Specialty,
            EmailDisplay = string.IsNullOrWhiteSpace(d.Email) ? "—" : d.Email,
            PhoneDisplay = string.IsNullOrWhiteSpace(d.Phone) ? "—" : d.Phone,
            BirthDateDisplay = birthDisplay,
            AgeDisplay = ageDisplay,
            AvailabilityLabel = avail,
            AvailabilityBadgeClass = badge
        };
    }

    private static (string BirthDate, string Age) FormatBirthAndAge(DateOnly? birthDate)
    {
        if (!birthDate.HasValue)
            return ("—", "—");
        var culture = CultureInfo.CurrentCulture;
        var birthStr = birthDate.Value.ToString("d", culture);
        var age = AgeYears(birthDate.Value);
        return (birthStr, age.HasValue ? age.Value.ToString(culture) : "—");
    }

    private static int? AgeYears(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (birthDate > today)
            return null;
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;
        return age >= 0 ? age : null;
    }

    private static (string Label, string BadgeClass) MapAvailabilityBadge(List<DoctorAvailability> slots, DateTime now)
    {
        if (slots.Count == 0)
            return ("No schedule", "bg-secondary-subtle text-secondary-emphasis");

        var dow = now.DayOfWeek;
        var todays = slots.Where(s => s.DayOfWeek == dow).ToList();
        if (todays.Count == 0)
            return ("Off duty", "bg-secondary-subtle text-secondary-emphasis");

        var t = TimeOnly.FromDateTime(now);
        if (todays.Any(s => t >= s.StartTime && t < s.EndTime))
            return ("Available now", "bg-success-subtle text-success-emphasis");

        return ("Outside hours", "bg-warning-subtle text-warning-emphasis");
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
            1 => "cc-avatar-purple",
            2 => "cc-avatar-emerald",
            _ => "cc-avatar-rose"
        };

    public bool Create(CreateDoctorViewModel model)
    {
        var entity = new Doctor
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Specialty = model.Specialty,
            LicenseNumber = model.LicenseNumber,
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            BirthDate = model.BirthDate,
            CreatedAt = DateTime.UtcNow
        };
        TryAssignApplicationUserFromEmail(entity);
        var doctorRepo = _unitOfWork.GetRepository<Doctor>();
        doctorRepo.Add(entity);
        if (_unitOfWork.SaveChanges() <= 0)
            return false;

        var doctorId = entity.Id;
        var avRepo = _unitOfWork.GetRepository<DoctorAvailability>();
        var utc = DateTime.UtcNow;

        foreach (var slot in model.AvailabilitySlots ?? [])
        {
            if (slot.IsEmpty() || slot.IsPartiallyFilled())
                continue;
            if (!slot.DayOfWeek.HasValue || !slot.StartTime.HasValue || !slot.EndTime.HasValue)
                continue;
            if (slot.EndTime.Value <= slot.StartTime.Value)
                continue;

            avRepo.Add(new DoctorAvailability
            {
                DoctorId = doctorId,
                DayOfWeek = slot.DayOfWeek.Value,
                StartTime = slot.StartTime.Value,
                EndTime = slot.EndTime.Value,
                CreatedAt = utc
            });
        }

        _unitOfWork.SaveChanges();
        return true;
    }

    public EditDoctorViewModel? GetDoctorForEdit(int id)
    {
        var d = _unitOfWork.GetRepository<Doctor>().GetById(id);
        if (d is null) return null;
        return new EditDoctorViewModel
        {
            Id = d.Id,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Specialty = d.Specialty,
            LicenseNumber = d.LicenseNumber,
            Email = d.Email,
            Phone = d.Phone,
            BirthDate = d.BirthDate
        };
    }

    public bool Update(EditDoctorViewModel model)
    {
        var repo = _unitOfWork.GetRepository<Doctor>();
        var d = repo.GetById(model.Id);
        if (d is null) return false;
        d.FirstName = model.FirstName;
        d.LastName = model.LastName;
        d.Specialty = model.Specialty;
        d.LicenseNumber = model.LicenseNumber;
        d.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        d.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        d.BirthDate = model.BirthDate;
        d.UpdatedAt = DateTime.UtcNow;
        TryAssignApplicationUserFromEmail(d);
        repo.Update(d);
        return _unitOfWork.SaveChanges() > 0;
    }

    public int? GetDoctorIdForApplicationUser(string applicationUserId, string? userEmail)
    {
        var repo = _unitOfWork.GetRepository<Doctor>();
        var all = repo.GetAll().ToList();
        var byFk = all.FirstOrDefault(d => d.ApplicationUserId == applicationUserId);
        if (byFk is not null)
            return byFk.Id;

        if (string.IsNullOrWhiteSpace(userEmail))
            return null;

        var byEmail = all.FirstOrDefault(d =>
            !string.IsNullOrWhiteSpace(d.Email) &&
            string.Equals(d.Email.Trim(), userEmail.Trim(), StringComparison.OrdinalIgnoreCase));
        if (byEmail is null)
            return null;

        if (byEmail.ApplicationUserId != applicationUserId)
        {
            byEmail.ApplicationUserId = applicationUserId;
            repo.Update(byEmail);
            _unitOfWork.SaveChanges();
        }

        return byEmail.Id;
    }

    public DoctorSchedulePickerViewModel GetDoctorSchedulePicker()
    {
        var items = _unitOfWork.GetRepository<Doctor>().GetAll()
            .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
            .Select(d => new SelectListItem(
                $"Dr. {d.FirstName} {d.LastName}".Trim(),
                d.Id.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        return new DoctorSchedulePickerViewModel { Doctors = items };
    }

    /// <summary>Sets <see cref="Doctor.ApplicationUserId"/> when a single AspNetUsers row has the same email; clears it when email is empty or no user matches.</summary>
    private void TryAssignApplicationUserFromEmail(Doctor d)
    {
        if (string.IsNullOrWhiteSpace(d.Email))
        {
            d.ApplicationUserId = null;
            return;
        }

        var user = _userManager.FindByEmailAsync(d.Email.Trim()).ConfigureAwait(false).GetAwaiter().GetResult();
        if (user is null)
        {
            d.ApplicationUserId = null;
            return;
        }

        var repo = _unitOfWork.GetRepository<Doctor>();
        var takenByOther = repo.GetAll().Any(x => x.ApplicationUserId == user.Id && x.Id != d.Id);
        if (takenByOther)
        {
            d.ApplicationUserId = null;
            return;
        }

        d.ApplicationUserId = user.Id;
    }

    public bool TryDelete(int id, out string? error)
    {
        error = null;
        var appts = _unitOfWork.GetRepository<Appointment>().GetAll();
        if (appts.Any(a => a.DoctorId == id))
        {
            error = "Remove or reassign appointments for this doctor before deleting.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<Doctor>();
        var d = repo.GetById(id);
        if (d is null)
        {
            error = "Doctor not found.";
            return false;
        }

        try
        {
            repo.Delete(d);
            return _unitOfWork.SaveChanges() > 0;
        }
        catch
        {
            error = "Could not delete this doctor.";
            return false;
        }
    }

    public DoctorAvailabilityPageViewModel? GetAvailabilityPage(int doctorId)
    {
        var d = _unitOfWork.GetRepository<Doctor>().GetById(doctorId);
        if (d is null) return null;

        var slots = _unitOfWork.GetRepository<DoctorAvailability>().GetAll()
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => DaySortOrder(a.DayOfWeek))
            .ThenBy(a => a.StartTime)
            .Select(a => new DoctorAvailabilitySlotViewModel
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek,
                DayLabel = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(a.DayOfWeek),
                StartDisplay = a.StartTime.ToString("h:mm tt", CultureInfo.InvariantCulture),
                EndDisplay = a.EndTime.ToString("h:mm tt", CultureInfo.InvariantCulture)
            })
            .ToList();

        return new DoctorAvailabilityPageViewModel
        {
            DoctorId = doctorId,
            DoctorDisplayName = $"Dr. {d.FirstName} {d.LastName}".Trim(),
            Slots = slots,
            AddForm = new AddDoctorAvailabilityViewModel { DoctorId = doctorId }
        };
    }

    public bool TryAddAvailability(AddDoctorAvailabilityViewModel model, out string? error)
    {
        error = null;
        var doctorRepo = _unitOfWork.GetRepository<Doctor>();
        if (doctorRepo.GetById(model.DoctorId) is null)
        {
            error = "Doctor not found.";
            return false;
        }

        if (model.EndTime <= model.StartTime)
        {
            error = "End time must be after start time.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<DoctorAvailability>();
        var duplicate = repo.GetAll().Any(a =>
            a.DoctorId == model.DoctorId &&
            a.DayOfWeek == model.DayOfWeek &&
            a.StartTime == model.StartTime &&
            a.EndTime == model.EndTime);
        if (duplicate)
        {
            error = "This availability slot already exists.";
            return false;
        }

        var entity = new DoctorAvailability
        {
            DoctorId = model.DoctorId,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            CreatedAt = DateTime.UtcNow
        };
        try
        {
            repo.Add(entity);
            return _unitOfWork.SaveChanges() > 0;
        }
        catch
        {
            error = "Could not save availability (duplicate or invalid data).";
            return false;
        }
    }

    public bool TryDeleteAvailability(int doctorId, int availabilityId, out string? error)
    {
        error = null;
        var repo = _unitOfWork.GetRepository<DoctorAvailability>();
        var row = repo.GetById(availabilityId);
        if (row is null || row.DoctorId != doctorId)
        {
            error = "Availability slot not found.";
            return false;
        }

        try
        {
            repo.Delete(row);
            return _unitOfWork.SaveChanges() > 0;
        }
        catch
        {
            error = "Could not remove availability.";
            return false;
        }
    }

    private static int DaySortOrder(DayOfWeek d) =>
        d switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => 7
        };
}
