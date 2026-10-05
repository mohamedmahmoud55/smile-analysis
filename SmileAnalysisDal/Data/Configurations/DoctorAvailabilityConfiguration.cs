using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");
        builder.HasIndex(x => new { x.DoctorId, x.DayOfWeek, x.StartTime, x.EndTime }).IsUnique();

        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.Availabilities)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
