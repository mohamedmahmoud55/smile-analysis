namespace SmileAnalysisDal.Entities;

public class Staff : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
