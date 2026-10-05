using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class PatientBillingAccountConfiguration : IEntityTypeConfiguration<PatientBillingAccount>
{
    public void Configure(EntityTypeBuilder<PatientBillingAccount> builder)
    {
        builder.ToTable("PatientBillingAccounts");
        builder.Property(x => x.BaseTreatmentCost).HasPrecision(12, 2);
        builder.Property(x => x.AdditionalCosts).HasPrecision(12, 2);
        builder.Property(x => x.DiscountPercent).HasPrecision(9, 4);
        builder.Property(x => x.DiscountFixedAmount).HasPrecision(12, 2);
        builder.HasIndex(x => x.PatientId).IsUnique();
        builder.HasOne(x => x.Patient)
            .WithOne(p => p.BillingAccount)
            .HasForeignKey<PatientBillingAccount>(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
