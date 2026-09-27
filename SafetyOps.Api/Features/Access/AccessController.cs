using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Auth;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Access;

/// <summary>The org tree and role assignments (Access Levels).</summary>
[Route("api/access")]
public class AccessController(IAccessService access) : ApiControllerBase
{
    /// <summary>Org units the signed-in user can see, with their effective role on each.</summary>
    [HttpGet("org-units")]
    [Authorize(Policy = AccessPolicies.Read)]
    [ProducesResponseType<IReadOnlyList<OrgUnitDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<OrgUnitDto>> OrgUnits(CancellationToken ct) =>
        await access.GetOrgUnitsAsync(ct);

    /// <summary>Role assignments on the org units the signed-in user administers.</summary>
    [HttpGet("assignments")]
    [Authorize(Policy = AccessPolicies.ManageRoles)]
    [ProducesResponseType<IReadOnlyList<RoleAssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<RoleAssignmentDto>> Assignments(CancellationToken ct) =>
        await access.ListAssignmentsAsync(ct);

    /// <summary>Accounts that roles can be granted to.</summary>
    [HttpGet("users")]
    [Authorize(Policy = AccessPolicies.ManageRoles)]
    [ProducesResponseType<IReadOnlyList<UserOptionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<UserOptionDto>> Users(CancellationToken ct) =>
        await access.ListUsersAsync(ct);

    /// <summary>Grants a role on an org unit the signed-in user administers.</summary>
    [HttpPost("assignments")]
    [Authorize(Policy = AccessPolicies.ManageRoles)]
    [ProducesResponseType<RoleAssignmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoleAssignmentDto>> Grant(GrantRoleRequest request, CancellationToken ct)
    {
        var result = await access.GrantAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Created($"/api/access/assignments/{result.Value!.Id}", result.Value);
    }

    /// <summary>Revokes a role assignment. You can't revoke your own.</summary>
    [HttpDelete("assignments/{id:int}")]
    [Authorize(Policy = AccessPolicies.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Revoke(int id, CancellationToken ct) =>
        NoContentOrFailure(await access.RevokeAsync(id, AuthService.GetUserId(User)!.Value, ct));
}
