using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Contexts;

public class SmileAnalysisDbContext : IdentityDbContext<ApplicationUser>
{
    public SmileAnalysisDbContext(DbContextOptions<SmileAnalysisDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<ApplicationUser>(eb =>
        {
            eb.Property(x => x.FirstName).HasColumnType("varchar").HasMaxLength(50);
            eb.Property(x => x.LastName).HasColumnType("varchar").HasMaxLength(50);
        });
    }

    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<PatientClinicalHistory> PatientClinicalHistories { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; } = null!;
    public DbSet<Staff> StaffMembers { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PatientBillingAccount> PatientBillingAccounts { get; set; } = null!;
    public DbSet<GummySmileCase> GummySmileCases { get; set; } = null!;
    public DbSet<XRayAnalysisCase> XRayAnalysisCases { get; set; } = null!;
    public DbSet<Prescription> Prescriptions { get; set; } = null!;
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; } = null!;
}
