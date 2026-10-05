using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.Property(i => i.MedicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Dosage)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Duration)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Instructions)
            .HasMaxLength(500);

        builder.HasOne(i => i.Prescription)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
