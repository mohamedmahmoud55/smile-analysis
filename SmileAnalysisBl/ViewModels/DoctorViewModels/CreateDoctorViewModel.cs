using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class CreateDoctorViewModel : IValidatableObject
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required, StringLength(200)]
    public string Specialty { get; set; } = null!;

    [Required, StringLength(80)]
    [RegularExpression(@"^\d+$", ErrorMessage = "License number must contain digits only.")]
    public string LicenseNumber { get; set; } = null!;

    [StringLength(256), Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(30), Display(Name = "Phone")]
    public string? Phone { get; set; }

    [DataType(DataType.Date), Display(Name = "Birth date")]
    public DateOnly? BirthDate { get; set; }

    /// <summary>Empty rows are ignored. Add more rows with “Add time slot” on the form.</summary>
    public List<DoctorAvailabilitySlotInputModel> AvailabilitySlots { get; set; } =
    [
        new DoctorAvailabilitySlotInputModel(),
        new DoctorAvailabilitySlotInputModel(),
        new DoctorAvailabilitySlotInputModel()
    ];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AvailabilitySlots is null || AvailabilitySlots.Count == 0)
            yield break;

        for (var i = 0; i < AvailabilitySlots.Count; i++)
        {
            var s = AvailabilitySlots[i];
            var prefix = $"AvailabilitySlots[{i}]";

            if (s.IsEmpty())
                continue;

            if (s.IsPartiallyFilled())
            {
                yield return new ValidationResult(
                    "For each time slot, choose day, start time, and end time (or clear the row).",
                    [$"{prefix}.DayOfWeek", $"{prefix}.StartTime", $"{prefix}.EndTime"]);
                continue;
            }

            if (s.EndTime <= s.StartTime)
                yield return new ValidationResult("End time must be after start time.", [$"{prefix}.EndTime"]);
        }

        var complete = AvailabilitySlots.Where(s => !s.IsEmpty() && !s.IsPartiallyFilled()).ToList();
        var dupKeys = complete
            .Select(s => (s.DayOfWeek!.Value, s.StartTime!.Value, s.EndTime!.Value))
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (dupKeys.Count > 0)
            yield return new ValidationResult("Duplicate time slots (same day and times) are not allowed.");
    }
}
