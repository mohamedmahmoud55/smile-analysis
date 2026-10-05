using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

/// <summary>One optional weekly slot on the create-doctor form. Leave all empty to skip.</summary>
public class DoctorAvailabilitySlotInputModel
{
    [Display(Name = "Day")]
    public DayOfWeek? DayOfWeek { get; set; }

    [Display(Name = "Start")]
    [DataType(DataType.Time)]
    public TimeOnly? StartTime { get; set; }

    [Display(Name = "End")]
    [DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; }

    public bool IsEmpty() =>
        !DayOfWeek.HasValue && !StartTime.HasValue && !EndTime.HasValue;

    public bool IsPartiallyFilled() =>
        !IsEmpty() && (!DayOfWeek.HasValue || !StartTime.HasValue || !EndTime.HasValue);
}
