using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.PrescriptionViewModels;

public class PrescriptionItemInputViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Medication name is required.")]
    [StringLength(200, ErrorMessage = "Medication name cannot exceed 200 characters.")]
    [Display(Name = "Medication")]
    public string MedicationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dosage is required.")]
    [StringLength(100, ErrorMessage = "Dosage cannot exceed 100 characters.")]
    [Display(Name = "Dosage")]
    public string Dosage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration is required.")]
    [StringLength(100, ErrorMessage = "Duration cannot exceed 100 characters.")]
    [Display(Name = "Duration")]
    public string Duration { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
    [Display(Name = "Instructions")]
    public string? Instructions { get; set; }
}
