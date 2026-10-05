using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class AddDoctorAvailabilityViewModel : IValidatableObject
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    [Display(Name = "Day")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Display(Name = "Start time")]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [Required]
    [Display(Name = "End time")]
    [DataType(DataType.Time)]
    public TimeOnly EndTime { get; set; } = new(17, 0);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
            yield return new ValidationResult("End time must be after start time.", [nameof(EndTime)]);
    }
}
