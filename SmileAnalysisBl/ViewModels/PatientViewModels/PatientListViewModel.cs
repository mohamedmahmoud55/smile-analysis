using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class PatientListViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
