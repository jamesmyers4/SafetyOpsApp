using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class TrainingClassConfiguration : IEntityTypeConfiguration<TrainingClass>
{
    public void Configure(EntityTypeBuilder<TrainingClass> builder)
    {
        builder.Property(c => c.Location).HasMaxLength(200).IsRequired();
        builder.HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.CourseId, c.ClassDate });

        builder.Property(x => x.OrgUnitId).HasDefaultValue(OrgUnit.RootId);
        builder.HasOne(x => x.OrgUnit).WithMany().HasForeignKey(x => x.OrgUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
