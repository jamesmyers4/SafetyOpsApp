namespace SafetyOps.Api.Domain;

/// <summary>An account that can sign in. The password is stored only as a salted hash.</summary>
public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
