using System.ComponentModel.DataAnnotations;
using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class EditPatientViewModel
{
    [Required]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    public PatientClinicalHistoryInputModel Clinical { get; set; } = new();
}
