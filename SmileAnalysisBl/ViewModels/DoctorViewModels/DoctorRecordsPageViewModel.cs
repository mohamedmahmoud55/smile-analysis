namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class DoctorRecordsPageViewModel
{
    public string? SearchQuery { get; set; }
    public int TotalStaffCount { get; set; }
    public int TotalDoctors { get; set; }
    public int MatchingCount { get; set; }
    public int OnDutyApprox { get; set; }
    public string PatientRatioDisplay { get; set; } = "—";
    public int SatisfactionPercent { get; set; } = 98;
    public IReadOnlyList<DoctorDirectoryRowViewModel> Rows { get; set; } = [];
}

public class DoctorDirectoryRowViewModel
{
    public int Id { get; set; }
    public string DisplayId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarClass { get; set; } = "cc-avatar-blue";
    public string Specialty { get; set; } = string.Empty;
    public string EmailDisplay { get; set; } = string.Empty;
    public string PhoneDisplay { get; set; } = string.Empty;
    public string BirthDateDisplay { get; set; } = "—";
    public string AgeDisplay { get; set; } = "—";
    public string AvailabilityLabel { get; set; } = string.Empty;
    public string AvailabilityBadgeClass { get; set; } = "bg-success-subtle text-success";
}
