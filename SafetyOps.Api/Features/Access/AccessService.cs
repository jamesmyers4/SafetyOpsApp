using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Auth;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Access;

public interface IAccessService
{
    Task<IReadOnlyList<OrgUnitDto>> GetOrgUnitsAsync(CancellationToken ct = default);
    Task<AccessSummaryDto> GetSummaryAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleAssignmentDto>> ListAssignmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserOptionDto>> ListUsersAsync(CancellationToken ct = default);
    Task<Result<RoleAssignmentDto>> GrantAsync(GrantRoleRequest request, CancellationToken ct = default);
    Task<Result> RevokeAsync(int assignmentId, int currentUserId, CancellationToken ct = default);
}

public sealed class AccessService(AppDbContext db, IAccessScope scope) : IAccessService
{
    public static readonly ServiceError AssignmentNotFound = ServiceError.NotFound("Role assignment not found.");

    public async Task<IReadOnlyList<OrgUnitDto>> GetOrgUnitsAsync(CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        return access.Units
            .Where(u => access.RoleOn(u.Id) is not null)
            .OrderBy(u => u.Id)
            .Select(u => new OrgUnitDto(u.Id, u.Code, u.Name, u.ParentId, access.RoleOn(u.Id)!.Value))
            .ToList();
    }

    public async Task<AccessSummaryDto> GetSummaryAsync(int userId, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var grants = await db.RoleAssignments.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.OrgUnitId)
            .Select(a => new AccessGrantDto(a.OrgUnitId, a.OrgUnit.Name, a.Role))
            .ToListAsync(ct);
        return new AccessSummaryDto(access.HasAnywhere(Role.Viewer), access.HasAnywhere(Role.Manager), access.HasAnywhere(Role.Admin), grants);
    }

    public async Task<IReadOnlyList<RoleAssignmentDto>> ListAssignmentsAsync(CancellationToken ct = default)
    {
        var adminUnits = (await scope.GetAsync(ct)).UnitsWith(Role.Admin);
        return await db.RoleAssignments.AsNoTracking()
            .Where(a => adminUnits.Contains(a.OrgUnitId))
            .OrderBy(a => a.OrgUnitId).ThenBy(a => a.User.UserName)
            .Select(a => new RoleAssignmentDto(a.Id, a.UserId, a.User.UserName, a.User.DisplayName, a.OrgUnitId, a.OrgUnit.Name, a.Role))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserOptionDto>> ListUsersAsync(CancellationToken ct = default) =>
        await db.Users.AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserOptionDto(u.Id, u.UserName, u.DisplayName))
            .ToListAsync(ct);

    public async Task<Result<RoleAssignmentDto>> GrantAsync(GrantRoleRequest request, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        if (access.Units.All(u => u.Id != request.OrgUnitId))
            return ServiceError.Invalid("orgUnitId", $"Unknown org unit {request.OrgUnitId}.");
        if (!access.Can(request.OrgUnitId, Role.Admin))
            return ServiceError.Forbidden("You can only grant roles on org units you administer.");
        if (!await db.Users.AnyAsync(u => u.Id == request.UserId, ct))
            return ServiceError.Invalid("userId", $"Unknown user {request.UserId}.");
        if (await db.RoleAssignments.AnyAsync(a => a.UserId == request.UserId && a.OrgUnitId == request.OrgUnitId, ct))
            return ServiceError.Conflict("This user already has a role on this org unit. Revoke it first to change it.");

        var assignment = new RoleAssignment { UserId = request.UserId, OrgUnitId = request.OrgUnitId, Role = request.Role!.Value };
        db.RoleAssignments.Add(assignment);
        await db.SaveChangesAsync(ct);

        return await db.RoleAssignments.AsNoTracking()
            .Where(a => a.Id == assignment.Id)
            .Select(a => new RoleAssignmentDto(a.Id, a.UserId, a.User.UserName, a.User.DisplayName, a.OrgUnitId, a.OrgUnit.Name, a.Role))
            .SingleAsync(ct);
    }

    public async Task<Result> RevokeAsync(int assignmentId, int currentUserId, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var assignment = await db.RoleAssignments.FindAsync([assignmentId], ct);
        if (assignment is null || !access.Can(assignment.OrgUnitId, Role.Admin))
            return AssignmentNotFound;
        if (assignment.UserId == currentUserId)
            return ServiceError.Conflict("You can't revoke your own role. Ask another administrator.");

        db.RoleAssignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
        return Result.Success;
    }
}
