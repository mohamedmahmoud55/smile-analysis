namespace SmileAnalysisBl.ViewModels.AnalyticsViewModels;

public class OrthoDashboardViewModel
{
    public string WelcomeName { get; set; } = "Doctor";
    public int TodayAppointments { get; set; }
    public int NewConsultationsToday { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedThisWeek { get; set; }
    public int CompletedDeltaVsLastWeek { get; set; }
    public IReadOnlyList<RecentPatientRowViewModel> RecentPatients { get; set; } = Array.Empty<RecentPatientRowViewModel>();
}

public class RecentPatientRowViewModel
{
    public int Id { get; set; }
    public string DisplayId => $"P-{Id:D4}";
    public string FullName { get; set; } = "";
    public int Age { get; set; }
    public string LastVisitDisplay { get; set; } = "";
    /// <summary>Bootstrap-friendly: primary, warning, success, ortho-purple, info, secondary</summary>
    public string StatusBadgeVariant { get; set; } = "primary";
    public string StatusLabel { get; set; } = "";
}
