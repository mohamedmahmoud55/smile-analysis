using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.DoctorViewModels;

public class EditDoctorViewModel
{
    [Required]
    public int Id { get; set; }

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
}
