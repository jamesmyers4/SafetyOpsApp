using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Features.Auth;

/// <summary>
/// Creates the configured demo accounts that don't exist yet (with hashed passwords) and makes
/// sure each has its configured role. Existing passwords and extra roles are left alone.
/// </summary>
public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext db, AuthOptions options, IPasswordHasher<AppUser> hasher, CancellationToken ct = default)
    {
        foreach (var demo in options.DemoUsers.Where(d => !string.IsNullOrWhiteSpace(d.UserName) && !string.IsNullOrEmpty(d.Password)))
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == demo.UserName, ct);
            if (user is null)
            {
                user = new AppUser
                {
                    UserName = demo.UserName,
                    DisplayName = string.IsNullOrWhiteSpace(demo.DisplayName) ? demo.UserName : demo.DisplayName,
                };
                user.PasswordHash = hasher.HashPassword(user, demo.Password);
                db.Users.Add(user);
                await db.SaveChangesAsync(ct);
            }

            if (demo.Role is { } role && !string.IsNullOrWhiteSpace(demo.OrgUnit))
            {
                var unit = await db.OrgUnits.FirstOrDefaultAsync(u => u.Code == demo.OrgUnit, ct)
                    ?? throw new InvalidOperationException($"Auth:DemoUsers: unknown org unit '{demo.OrgUnit}' for '{demo.UserName}'.");
                if (!await db.RoleAssignments.AnyAsync(a => a.UserId == user.Id && a.OrgUnitId == unit.Id, ct))
                {
                    db.RoleAssignments.Add(new RoleAssignment { UserId = user.Id, OrgUnitId = unit.Id, Role = role });
                    await db.SaveChangesAsync(ct);
                }
            }
        }
    }
}
