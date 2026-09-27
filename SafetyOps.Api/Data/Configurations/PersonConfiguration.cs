using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MiddleName).HasMaxLength(100);
        builder.Property(p => p.Gender).HasMaxLength(50);
        builder.Property(p => p.Department).HasMaxLength(100);
        builder.Property(p => p.EmployeeCategory).HasMaxLength(50);
        builder.Property(p => p.Subscription).HasMaxLength(50);
        builder.Property(p => p.EmployeeNumber).HasMaxLength(20);
        builder.Ignore(p => p.FullName);
        builder.HasIndex(p => new { p.LastName, p.FirstName });

        builder.Property(x => x.OrgUnitId).HasDefaultValue(OrgUnit.RootId);
        builder.HasOne(x => x.OrgUnit).WithMany().HasForeignKey(x => x.OrgUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
