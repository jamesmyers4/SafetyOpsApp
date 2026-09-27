using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasMany(t => t.Stressors)
            .WithMany()
            .UsingEntity(
                "WorkTaskStressor",
                r => r.HasOne(typeof(Stressor)).WithMany().HasForeignKey("StressorId"),
                l => l.HasOne(typeof(WorkTask)).WithMany().HasForeignKey("WorkTaskId"),
                j =>
                {
                    j.HasKey("WorkTaskId", "StressorId");
                    j.HasData(
                        new { WorkTaskId = 1, StressorId = 1 },
                        new { WorkTaskId = 2, StressorId = 2 },
                        new { WorkTaskId = 3, StressorId = 3 });
                });

        builder.HasData(
            new WorkTask { Id = 1, Code = "WT-001", Name = "Chemical Exposure - Solvents", ExamTypeOptions = ["Initial", "Periodic", "Exit", "Return to Duty"] },
            new WorkTask { Id = 2, Code = "WT-002", Name = "Noise Hazard - Industrial", ExamTypeOptions = ["Initial", "Periodic", "Exit"] },
            new WorkTask { Id = 3, Code = "WT-003", Name = "Respiratory Hazard - Dust", ExamTypeOptions = ["Initial", "Periodic", "Exit", "Special"] });
    }
}
