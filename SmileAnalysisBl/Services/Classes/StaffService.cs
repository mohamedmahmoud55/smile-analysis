using System.Globalization;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.StaffViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class StaffService : IStaffService
{
    private readonly IUnitOfWork _unitOfWork;

    public StaffService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public StaffRecordsPageViewModel GetStaffRegistryPage(string? search = null)
    {
        var allStaff = _unitOfWork.GetRepository<Staff>().GetAll().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

        var displayed = allStaff;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            displayed = allStaff
                .Where(s =>
                    (s.FirstName + " " + s.LastName).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    s.Id.ToString(CultureInfo.InvariantCulture).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(s.Email) && s.Email.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Phone) && s.Phone.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var total = allStaff.Count;
        var onDuty = total == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(total * 0.35));
        var openShifts = total == 0 ? 0 : (total % 5) + 2;

        var rows = displayed.Select(MapRow).ToList();

        return new StaffRecordsPageViewModel
        {
            SearchQuery = search,
            TotalStaff = total,
            MatchingCount = displayed.Count,
            OnDutyNow = onDuty,
            OpenShifts = openShifts,
            CompliancePercent = 100,
            Rows = rows
        };
    }

    private static StaffDirectoryRowViewModel MapRow(Staff s)
    {
        var (role, roleClass) = RoleFor(s.Id);
        var (days, hours) = ShiftFor(s.Id);
        var (birthDisplay, ageDisplay) = FormatBirthAndAge(s.BirthDate);

        return new StaffDirectoryRowViewModel
        {
            Id = s.Id,
            DisplayId = $"S-{s.Id:0000}",
            FullName = $"{s.FirstName} {s.LastName}".Trim(),
            Initials = Initials(s.FirstName, s.LastName),
            AvatarClass = AvatarClassFor(s.Id),
            RoleLabel = role,
            RoleBadgeClass = roleClass,
            EmailDisplay = string.IsNullOrWhiteSpace(s.Email) ? "—" : s.Email,
            PhoneDisplay = string.IsNullOrWhiteSpace(s.Phone) ? "—" : s.Phone,
            BirthDateDisplay = birthDisplay,
            AgeDisplay = ageDisplay,
            ShiftDays = days,
            ShiftHours = hours
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
            1 => "cc-avatar-amber",
            2 => "cc-avatar-slate",
            _ => "cc-avatar-blue"
        };

    private static (string Role, string BadgeClass) RoleFor(int id) =>
        (Math.Abs(id) % 4) switch
        {
            0 => ("Hygienist", "bg-primary-subtle text-primary-emphasis"),
            1 => ("Assistant", "bg-warning-subtle text-warning-emphasis"),
            2 => ("Receptionist", "bg-secondary-subtle text-secondary-emphasis"),
            _ => ("Hygienist", "bg-primary-subtle text-primary-emphasis")
        };

    private static (string Days, string Hours) ShiftFor(int id) =>
        (Math.Abs(id) % 3) switch
        {
            0 => ("Mon - Fri", "08:00 AM - 04:00 PM"),
            1 => ("Tue - Sat", "10:00 AM - 06:00 PM"),
            _ => ("Weekend only", "09:00 AM - 05:00 PM")
        };

    public bool Create(CreateStaffViewModel model)
    {
        var entity = new Staff
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            BirthDate = model.BirthDate,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.GetRepository<Staff>().Add(entity);
        return _unitOfWork.SaveChanges() > 0;
    }

    public EditStaffViewModel? GetStaffForEdit(int id)
    {
        var s = _unitOfWork.GetRepository<Staff>().GetById(id);
        if (s is null) return null;
        return new EditStaffViewModel
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            Phone = s.Phone,
            BirthDate = s.BirthDate
        };
    }

    public bool Update(EditStaffViewModel model)
    {
        var repo = _unitOfWork.GetRepository<Staff>();
        var s = repo.GetById(model.Id);
        if (s is null) return false;
        s.FirstName = model.FirstName;
        s.LastName = model.LastName;
        s.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        s.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        s.BirthDate = model.BirthDate;
        s.UpdatedAt = DateTime.UtcNow;
        repo.Update(s);
        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryDelete(int id, out string? error)
    {
        error = null;
        var appts = _unitOfWork.GetRepository<Appointment>().GetAll();
        if (appts.Any(a => a.StaffId == id))
        {
            error = "Remove or reassign appointments for this staff member before deleting.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<Staff>();
        var s = repo.GetById(id);
        if (s is null)
        {
            error = "Staff member not found.";
            return false;
        }

        try
        {
            repo.Delete(s);
            return _unitOfWork.SaveChanges() > 0;
        }
        catch
        {
            error = "Could not delete this staff member.";
            return false;
        }
    }
}
