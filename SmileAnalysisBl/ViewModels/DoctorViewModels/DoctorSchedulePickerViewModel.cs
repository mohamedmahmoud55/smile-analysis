using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class DoctorSchedulePickerViewModel
{
    public IEnumerable<SelectListItem> Doctors { get; set; } = [];
}
