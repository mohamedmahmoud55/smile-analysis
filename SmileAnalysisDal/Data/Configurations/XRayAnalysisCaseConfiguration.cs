using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class XRayAnalysisCaseConfiguration : IEntityTypeConfiguration<XRayAnalysisCase>
{
    public void Configure(EntityTypeBuilder<XRayAnalysisCase> builder)
    {
        builder.ToTable("XRayAnalysisCases");
        builder.HasIndex(x => x.PatientId);

        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.CvmStage).HasMaxLength(32);
        builder.Property(x => x.SkeletalClass).HasMaxLength(64);
        builder.Property(x => x.ResultsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.PanoramicImageContentType).HasMaxLength(128);
        builder.Property(x => x.CephImageContentType).HasMaxLength(128);

        builder.HasOne(x => x.Patient)
            .WithMany(p => p.XRayAnalysisCases)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
