using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.Property(p => p.Amount).HasPrecision(12, 2);
            builder.HasOne(p => p.Appointment)
                .WithMany(a => a.Payments)
                .HasForeignKey(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.HasOne(p => p.Patient)
                .WithMany(pt => pt.Payments)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
