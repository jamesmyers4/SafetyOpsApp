using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Access;

/// <summary>
/// The rules every module applies to records owned by an org unit:
/// outside the user's read scope a record doesn't exist (404); inside it but without
/// Manager, changes are refused (403); new or moved records must land in a unit the user can write.
/// </summary>
public static class AccessRules
{
    public static readonly ServiceError ReadOnly =
        ServiceError.Forbidden("Your role on this org unit is read-only.");

    public static bool CanRead(this AccessSnapshot access, int orgUnitId) => access.Can(orgUnitId, Role.Viewer);

    public static bool CanWrite(this AccessSnapshot access, int orgUnitId) => access.Can(orgUnitId, Role.Manager);

    /// <summary>
    /// Picks the unit a created or updated record belongs to (the requested one, else its current
    /// one, else the user's default) and checks the user may write there.
    /// </summary>
    public static Result<int> ResolveWriteUnit(this AccessSnapshot access, int? requested, int? current = null)
    {
        if ((requested ?? current ?? access.DefaultWriteUnit) is not { } unitId)
            return ServiceError.Forbidden("You don't have a role that allows creating records.");
        if (access.Units.All(u => u.Id != unitId))
            return ServiceError.Invalid("orgUnitId", $"Unknown org unit {unitId}.");
        if (!access.CanWrite(unitId))
            return ServiceError.Forbidden("You can't make changes in this org unit.");
        return unitId;
    }
}
