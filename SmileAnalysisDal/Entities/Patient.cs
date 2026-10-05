using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisDal.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public PatientClinicalHistory? ClinicalHistory { get; set; }

    public PatientBillingAccount? BillingAccount { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<GummySmileCase> GummySmileCases { get; set; } = new List<GummySmileCase>();
    public ICollection<XRayAnalysisCase> XRayAnalysisCases { get; set; } = new List<XRayAnalysisCase>();
}
