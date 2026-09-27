namespace SafetyOps.Api.Features.Auth;

/// <summary>The <c>Auth</c> configuration section.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Accounts created at startup if they don't exist yet. Development configuration defines the
    /// <c>admin</c> demo login; other environments supply their own through configuration
    /// (environment variables or user secrets), never source control.
    /// </summary>
    public List<DemoUserOptions> DemoUsers { get; set; } = [];
}

public sealed class DemoUserOptions
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
