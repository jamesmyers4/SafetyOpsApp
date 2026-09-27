using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class OrgUnitConfiguration : IEntityTypeConfiguration<OrgUnit>
{
    public void Configure(EntityTypeBuilder<OrgUnit> builder)
    {
        builder.Property(u => u.Code).HasMaxLength(20).IsRequired();
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(u => u.Code).IsUnique();
        builder.HasOne(u => u.Parent).WithMany().HasForeignKey(u => u.ParentId).OnDelete(DeleteBehavior.Restrict);

        // Organization -> 2 divisions -> 2 sites each. Ships with the migration so every
        // environment has at least the root unit to assign roles on.
        builder.HasData(
            new OrgUnit { Id = OrgUnit.RootId, Code = "ORG", Name = "SafetyOps Industries" },
            new OrgUnit { Id = 2, Code = "MFG", Name = "Manufacturing Division", ParentId = OrgUnit.RootId },
            new OrgUnit { Id = 3, Code = "LOG", Name = "Logistics Division", ParentId = OrgUnit.RootId },
            new OrgUnit { Id = 4, Code = "MFG-N", Name = "North Plant", ParentId = 2 },
            new OrgUnit { Id = 5, Code = "MFG-S", Name = "South Plant", ParentId = 2 },
            new OrgUnit { Id = 6, Code = "LOG-E", Name = "East Warehouse", ParentId = 3 },
            new OrgUnit { Id = 7, Code = "LOG-W", Name = "West Warehouse", ParentId = 3 });
    }
}

public class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.Property(a => a.Role).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(a => new { a.UserId, a.OrgUnitId }).IsUnique();
        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.OrgUnit).WithMany().HasForeignKey(a => a.OrgUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
