using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class StressorConfiguration : IEntityTypeConfiguration<Stressor>
{
    public void Configure(EntityTypeBuilder<Stressor> builder)
    {
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();

        builder.HasData(
            new Stressor { Id = 1, Code = "STR-001", Name = "Solvent Exposure" },
            new Stressor { Id = 2, Code = "STR-002", Name = "Noise Exposure" },
            new Stressor { Id = 3, Code = "STR-003", Name = "Dust Inhalation" });
    }
}
