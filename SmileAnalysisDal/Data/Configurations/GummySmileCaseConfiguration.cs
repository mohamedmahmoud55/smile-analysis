using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class GummySmileCaseConfiguration : IEntityTypeConfiguration<GummySmileCase>
{
    public void Configure(EntityTypeBuilder<GummySmileCase> builder)
    {
        builder.ToTable("GummySmileCases");
        builder.HasIndex(x => x.GsCaseId).IsUnique();
        builder.HasIndex(x => x.PatientId);

        builder.Property(x => x.GsCaseId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.GsStatus).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DiagnosisSummary).HasMaxLength(500);
        builder.Property(x => x.Severity).HasMaxLength(50);
        builder.Property(x => x.ReportJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ClinicalDraftJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ClinicalRequestJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OverridesRequestJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CaseSnapshotJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CachedAssetsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RestImageContentType).HasMaxLength(128);
        builder.Property(x => x.SmileImageContentType).HasMaxLength(128);

        builder.HasOne(x => x.Patient)
            .WithMany(p => p.GummySmileCases)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
