namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class PatientRecordsPageViewModel
{
    public string? SearchQuery { get; set; }
    public int TotalPatients { get; set; }
    public int MatchingCount { get; set; }
    public int ScheduledToday { get; set; }
    public int UpcomingAppointments { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PatientGrowthPercent { get; set; }
    public IReadOnlyList<PatientDirectoryRowViewModel> Rows { get; set; } = [];

    /// <summary>When true, UI shows copy for “my appointment patients” and doctor-only actions.</summary>
    public bool IsDoctorMyPatientsView { get; set; }
}

public class PatientDirectoryRowViewModel
{
    public int Id { get; set; }
    public string DisplayId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarClass { get; set; } = "cc-avatar-blue";
    public string ContactDisplay { get; set; } = string.Empty;
    public string LastVisitDisplay { get; set; } = "—";
    public string NextAppointmentLabel { get; set; } = "Not Scheduled";
    public string NextAppointmentBadgeClass { get; set; } = "cc-badge-muted";
}
