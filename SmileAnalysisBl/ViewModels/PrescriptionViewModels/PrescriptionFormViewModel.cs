using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.PrescriptionViewModels;

public class PrescriptionFormViewModel
{
    public int? PrescriptionId { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string PatientDisplayId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
    public bool IsEdit { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "Add at least one medication.")]
    public List<PrescriptionItemInputViewModel> Items { get; set; } = [new()];
}
