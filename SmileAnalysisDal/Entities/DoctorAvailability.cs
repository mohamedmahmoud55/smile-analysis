namespace SmileAnalysisDal.Entities;

/// <summary>One weekly recurring availability window for a doctor.</summary>
public class DoctorAvailability : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    /// <summary>Day of week in the clinic calendar (system <see cref="DayOfWeek"/>).</summary>
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
