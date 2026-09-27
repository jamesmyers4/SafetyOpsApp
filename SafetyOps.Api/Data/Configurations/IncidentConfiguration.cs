using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.Property(i => i.Location).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(2000).IsRequired();
        // Enums as readable strings, so the table makes sense without the code.
        builder.Property(i => i.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Severity).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(i => i.ReportedBy)
            .WithMany()
            .HasForeignKey(i => i.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OccurredAt);
        builder.HasIndex(i => i.Status);

        builder.Property(x => x.OrgUnitId).HasDefaultValue(OrgUnit.RootId);
        builder.HasOne(x => x.OrgUnit).WithMany().HasForeignKey(x => x.OrgUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
