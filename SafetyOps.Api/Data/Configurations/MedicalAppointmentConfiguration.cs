using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class MedicalAppointmentConfiguration : IEntityTypeConfiguration<MedicalAppointment>
{
    public void Configure(EntityTypeBuilder<MedicalAppointment> builder)
    {
        builder.HasOne(a => a.Person)
            .WithMany()
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(a => a.Stressors, s =>
        {
            s.ToTable("AppointmentStressors");
            s.WithOwner().HasForeignKey(x => x.AppointmentId);
            s.HasKey(x => new { x.AppointmentId, x.StressorId });
            s.HasOne(x => x.Stressor).WithMany().HasForeignKey(x => x.StressorId).OnDelete(DeleteBehavior.Restrict);
            s.Property(x => x.ExamType).HasMaxLength(50);
        });
    }
}
