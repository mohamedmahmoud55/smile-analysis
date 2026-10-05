namespace SmileAnalysisBl.ViewModels.StaffViewModels;

public class StaffRecordsPageViewModel
{
    public string? SearchQuery { get; set; }
    public int TotalStaff { get; set; }
    public int MatchingCount { get; set; }
    public int OnDutyNow { get; set; }
    public int OpenShifts { get; set; }
    public int CompliancePercent { get; set; } = 100;
    public IReadOnlyList<StaffDirectoryRowViewModel> Rows { get; set; } = [];
}

public class StaffDirectoryRowViewModel
{
    public int Id { get; set; }
    public string DisplayId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarClass { get; set; } = "cc-avatar-blue";
    public string RoleLabel { get; set; } = string.Empty;
    public string RoleBadgeClass { get; set; } = "bg-primary-subtle text-primary";
    public string EmailDisplay { get; set; } = "—";
    public string PhoneDisplay { get; set; } = "—";
    public string BirthDateDisplay { get; set; } = "—";
    public string AgeDisplay { get; set; } = "—";
    public string ShiftDays { get; set; } = "Mon - Fri";
    public string ShiftHours { get; set; } = "08:00 AM - 04:00 PM";
}
