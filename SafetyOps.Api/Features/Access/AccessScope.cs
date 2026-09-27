using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Auth;

namespace SafetyOps.Api.Features.Access;

/// <summary>
/// The signed-in user's effective role on every org unit. A role assigned on a unit applies to
/// that unit and all its descendants; where assignments overlap, the strongest wins.
/// </summary>
public sealed class AccessSnapshot
{
    private readonly Dictionary<int, Role> _effective;
    private readonly Dictionary<int, int> _depth;

    public AccessSnapshot(IReadOnlyCollection<OrgUnit> units, IEnumerable<(int OrgUnitId, Role Role)> assignments)
    {
        Units = units;
        var parents = units.ToDictionary(u => u.Id, u => u.ParentId);
        var assigned = assignments.GroupBy(a => a.OrgUnitId).ToDictionary(g => g.Key, g => g.Max(a => a.Role));

        _effective = [];
        _depth = [];
        foreach (var unit in units)
        {
            Role? best = null;
            var depth = 0;
            for (int? id = unit.Id; id is { } current; id = parents.GetValueOrDefault(current), depth++)
            {
                if (assigned.TryGetValue(current, out var role) && (best is null || role > best))
                    best = role;
            }
            _depth[unit.Id] = depth;
            if (best is { } effective)
                _effective[unit.Id] = effective;
        }
    }

    public IReadOnlyCollection<OrgUnit> Units { get; }

    public Role? RoleOn(int orgUnitId) => _effective.TryGetValue(orgUnitId, out var role) ? role : null;

    public bool Can(int orgUnitId, Role minimum) => RoleOn(orgUnitId) >= minimum;

    public bool HasAnywhere(Role minimum) => _effective.Values.Any(r => r >= minimum);

    /// <summary>Ids of every unit where the user has at least <paramref name="minimum"/>.</summary>
    public IReadOnlyCollection<int> UnitsWith(Role minimum) =>
        _effective.Where(e => e.Value >= minimum).Select(e => e.Key).ToList();

    /// <summary>Where new records go when the request doesn't name a unit: the highest unit the user can write to.</summary>
    public int? DefaultWriteUnit =>
        _effective.Where(e => e.Value >= Role.Manager)
            .OrderBy(e => _depth[e.Key]).ThenBy(e => e.Key)
            .Select(e => (int?)e.Key)
            .FirstOrDefault();
}

public interface IAccessScope
{
    /// <summary>Loads (once per request) the signed-in user's access.</summary>
    Task<AccessSnapshot> GetAsync(CancellationToken ct = default);
}

public sealed class AccessScope(AppDbContext db, IHttpContextAccessor http) : IAccessScope
{
    private AccessSnapshot? _snapshot;

    public async Task<AccessSnapshot> GetAsync(CancellationToken ct = default)
    {
        if (_snapshot is not null)
            return _snapshot;

        var units = await db.OrgUnits.AsNoTracking().ToListAsync(ct);
        var userId = http.HttpContext is { } context ? AuthService.GetUserId(context.User) : null;
        var assignments = userId is { } id
            ? await db.RoleAssignments.AsNoTracking()
                .Where(a => a.UserId == id)
                .Select(a => new { a.OrgUnitId, a.Role })
                .ToListAsync(ct)
            : [];

        return _snapshot = new AccessSnapshot(units, assignments.Select(a => (a.OrgUnitId, a.Role)));
    }
}
