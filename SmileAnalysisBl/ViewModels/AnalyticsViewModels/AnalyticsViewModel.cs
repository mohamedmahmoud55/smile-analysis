namespace SmileAnalysisBl.ViewModels.AnalyticsViewModels;

public class AnalyticsViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalStaff { get; set; }
    public int ScheduledAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}
