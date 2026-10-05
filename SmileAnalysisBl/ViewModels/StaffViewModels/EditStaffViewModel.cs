using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.StaffViewModels;

public class EditStaffViewModel
{
    [Required]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(100)]
    public string LastName { get; set; } = null!;

    [StringLength(256), Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(30), Display(Name = "Phone")]
    public string? Phone { get; set; }

    [DataType(DataType.Date), Display(Name = "Birth date")]
    public DateOnly? BirthDate { get; set; }
}
