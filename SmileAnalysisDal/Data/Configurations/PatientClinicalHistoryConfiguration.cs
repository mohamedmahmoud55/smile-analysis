using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class PatientClinicalHistoryConfiguration : IEntityTypeConfiguration<PatientClinicalHistory>
{
    public void Configure(EntityTypeBuilder<PatientClinicalHistory> builder)
    {
        builder.ToTable("PatientClinicalHistories");
        builder.HasIndex(x => x.PatientId).IsUnique();

        builder.Property(x => x.ChronicDiseases).HasMaxLength(4000);
        builder.Property(x => x.CurrentMedications).HasMaxLength(4000);
        builder.Property(x => x.Allergies).HasMaxLength(4000);
        builder.Property(x => x.PreviousSurgeries).HasMaxLength(4000);
        builder.Property(x => x.HeartOrBleedingConditions).HasMaxLength(4000);
        builder.Property(x => x.CurrentPainDetails).HasMaxLength(4000);
        builder.Property(x => x.PreviousDentalTreatments).HasMaxLength(4000);
        builder.Property(x => x.GumProblems).HasMaxLength(4000);
        builder.Property(x => x.OralHabits).HasMaxLength(4000);

        builder.HasOne(x => x.Patient)
            .WithOne(p => p.ClinicalHistory)
            .HasForeignKey<PatientClinicalHistory>(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
