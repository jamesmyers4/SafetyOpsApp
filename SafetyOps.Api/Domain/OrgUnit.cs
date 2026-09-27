namespace SafetyOps.Api.Domain;

/// <summary>A node in the organization tree (organization, division, site, ...).</summary>
public class OrgUnit
{
    /// <summary>The root of the seeded tree. Records created before org units existed belong here.</summary>
    public const int RootId = 1;

    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public OrgUnit? Parent { get; set; }
}

/// <summary>Access levels, weakest to strongest. Each includes everything the previous one allows.</summary>
public enum Role
{
    /// <summary>Read-only.</summary>
    Viewer = 1,

    /// <summary>Create, edit, and delete records.</summary>
    Manager = 2,

    /// <summary>Manager, plus granting and revoking roles.</summary>
    Admin = 3,
}

/// <summary>Gives a user a role on an org unit and, implicitly, on every unit below it.</summary>
public class RoleAssignment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int OrgUnitId { get; set; }
    public OrgUnit OrgUnit { get; set; } = null!;
    public Role Role { get; set; }
}
