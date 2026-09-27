using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");
        // NOCASE makes both the unique index and sign-in lookups case-insensitive.
        builder.Property(u => u.UserName).HasMaxLength(50).UseCollation("NOCASE").IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
