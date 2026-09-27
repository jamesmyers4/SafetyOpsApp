using System.ComponentModel.DataAnnotations;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Features.Access;

/// <summary>An org unit the user can see, with their effective role on it.</summary>
public sealed record OrgUnitDto(int Id, string Code, string Name, int? ParentId, Role MyRole);

/// <summary>A role granted to a user on an org unit.</summary>
public sealed record RoleAssignmentDto(int Id, int UserId, string UserName, string DisplayName, int OrgUnitId, string OrgUnitName, Role Role);

/// <summary>Grants <see cref="Role"/> on an org unit (and everything below it).</summary>
public sealed record GrantRoleRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "User is required.")]
    public int UserId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Org unit is required.")]
    public int OrgUnitId { get; init; }

    [Required, EnumDataType(typeof(Role))]
    public Role? Role { get; init; }
}

/// <summary>An account that roles can be granted to.</summary>
public sealed record UserOptionDto(int Id, string UserName, string DisplayName);

/// <summary>What the signed-in user may do, for showing or hiding UI actions.</summary>
public sealed record AccessSummaryDto(bool CanRead, bool CanWrite, bool CanManageRoles, IReadOnlyList<AccessGrantDto> Grants);

/// <summary>One of the signed-in user's own role assignments.</summary>
public sealed record AccessGrantDto(int OrgUnitId, string OrgUnitName, Role Role);
