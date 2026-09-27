using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Features.Auth;

/// <summary>Creates the configured demo accounts that don't exist yet, storing hashed passwords.</summary>
public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext db, AuthOptions options, IPasswordHasher<AppUser> hasher, CancellationToken ct = default)
    {
        foreach (var demo in options.DemoUsers.Where(d => !string.IsNullOrWhiteSpace(d.UserName) && !string.IsNullOrEmpty(d.Password)))
        {
            if (await db.Users.AnyAsync(u => u.UserName == demo.UserName, ct))
                continue;

            var user = new AppUser
            {
                UserName = demo.UserName,
                DisplayName = string.IsNullOrWhiteSpace(demo.DisplayName) ? demo.UserName : demo.DisplayName,
            };
            user.PasswordHash = hasher.HashPassword(user, demo.Password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}
