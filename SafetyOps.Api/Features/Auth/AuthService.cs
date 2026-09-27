using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Access;

namespace SafetyOps.Api.Features.Auth;

public interface IAuthService
{
    /// <summary>Returns the user if the username and password match, otherwise null.</summary>
    Task<AppUser?> ValidateCredentialsAsync(string userName, string password, CancellationToken ct = default);

    Task<UserOptionDto?> GetUserAsync(int id, CancellationToken ct = default);
}

public sealed class AuthService(AppDbContext db, IPasswordHasher<AppUser> hasher) : IAuthService
{
    // Verifying against a throwaway hash when the user doesn't exist keeps the response time
    // the same for unknown users and wrong passwords, so timing doesn't reveal valid usernames.
    private static readonly AppUser Nobody = new();
    private static readonly string DummyHash = new PasswordHasher<AppUser>().HashPassword(Nobody, Guid.NewGuid().ToString());

    public async Task<AppUser?> ValidateCredentialsAsync(string userName, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName.Trim(), ct);
        var result = hasher.VerifyHashedPassword(user ?? Nobody, user?.PasswordHash ?? DummyHash, password);
        if (user is null || result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            await db.SaveChangesAsync(ct);
        }
        return user;
    }

    public Task<UserOptionDto?> GetUserAsync(int id, CancellationToken ct = default) =>
        db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserOptionDto(u.Id, u.UserName, u.DisplayName))
            .FirstOrDefaultAsync(ct);

    /// <summary>The cookie carries identity only. Permissions are looked up per request so changes apply immediately.</summary>
    public static ClaimsPrincipal CreatePrincipal(AppUser user) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.UserName),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme));

    public static int? GetUserId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
