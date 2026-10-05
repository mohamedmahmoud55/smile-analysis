using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.LastName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Specialty).HasMaxLength(200).IsRequired();
        builder.Property(d => d.LicenseNumber).HasMaxLength(80).IsRequired();
        builder.Property(d => d.Email).HasMaxLength(256);
        builder.Property(d => d.Phone).HasMaxLength(30);
        builder.Property(d => d.BirthDate).HasColumnType("date");

        builder.Property(d => d.ApplicationUserId).HasMaxLength(450);
        builder.HasOne(d => d.ApplicationUser)
            .WithMany()
            .HasForeignKey(d => d.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(d => d.ApplicationUserId)
            .IsUnique()
            .HasFilter("[ApplicationUserId] IS NOT NULL");
    }
}
