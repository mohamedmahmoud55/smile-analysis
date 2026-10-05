namespace SmileAnalysisDal.Entities;

public class Doctor : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Specialty { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }

    /// <summary>Optional link to the login account when doctor email matches the user email.</summary>
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
