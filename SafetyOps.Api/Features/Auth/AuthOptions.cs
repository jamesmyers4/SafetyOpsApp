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

    /// <summary>Optional role to grant (Viewer, Manager, Admin) on <see cref="OrgUnit"/>.</summary>
    public Domain.Role? Role { get; set; }

    /// <summary>Code of the org unit for <see cref="Role"/>, e.g. <c>ORG</c> or <c>MFG-N</c>.</summary>
    public string? OrgUnit { get; set; }
}
