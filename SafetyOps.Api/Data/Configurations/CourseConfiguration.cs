using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        // Reference data ships with the migration.
        builder.HasData(
            new Course { Id = 1, Code = "ELV-001", Title = "Electrical - Low Voltage" },
            new Course { Id = 2, Code = "ELH-001", Title = "Electrical - High Voltage" },
            new Course { Id = 3, Code = "ELS-001", Title = "Electrical - Safety Basics" },
            new Course { Id = 4, Code = "FPS-001", Title = "Fire Prevention and Safety" },
            new Course { Id = 5, Code = "HAZ-001", Title = "Hazardous Materials Handling" },
            new Course { Id = 6, Code = "PPE-001", Title = "Personal Protective Equipment" },
            new Course { Id = 7, Code = "FAC-001", Title = "First Aid and CPR" },
            new Course { Id = 8, Code = "LOT-001", Title = "Lockout/Tagout Procedures" });
    }
}
