namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class DoctorAvailabilityPageViewModel
{
    public int DoctorId { get; set; }
    public string DoctorDisplayName { get; set; } = string.Empty;
    public IReadOnlyList<DoctorAvailabilitySlotViewModel> Slots { get; set; } = [];
    public AddDoctorAvailabilityViewModel AddForm { get; set; } = new();
}

public class DoctorAvailabilitySlotViewModel
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string StartDisplay { get; set; } = string.Empty;
    public string EndDisplay { get; set; } = string.Empty;
}
