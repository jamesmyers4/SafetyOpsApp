using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Access;

namespace SafetyOps.Api.Tests.Unit;

public class AccessSnapshotTests
{
    // 1 ─┬─ 2 ─┬─ 4
    //    │     └─ 5
    //    └─ 3 ─── 6 ── 8
    private static readonly OrgUnit[] Tree =
    [
        new() { Id = 1 },
        new() { Id = 2, ParentId = 1 },
        new() { Id = 3, ParentId = 1 },
        new() { Id = 4, ParentId = 2 },
        new() { Id = 5, ParentId = 2 },
        new() { Id = 6, ParentId = 3 },
        new() { Id = 8, ParentId = 6 },
    ];

    private static AccessSnapshot Snapshot(params (int Unit, Role Role)[] assignments) => new(Tree, assignments);

    [Test]
    public void A_role_applies_to_the_unit_and_all_descendants_only()
    {
        var access = Snapshot((3, Role.Manager));

        Assert.That(new[] { 1, 2, 3, 4, 5, 6, 8 }.Select(access.RoleOn),
            Is.EqualTo(new Role?[] { null, null, Role.Manager, null, null, Role.Manager, Role.Manager }));
    }

    [Test]
    public void The_strongest_role_on_the_path_wins_in_either_direction()
    {
        var access = Snapshot((1, Role.Viewer), (3, Role.Admin), (6, Role.Manager));

        Assert.That(access.RoleOn(2), Is.EqualTo(Role.Viewer));
        Assert.That(access.RoleOn(6), Is.EqualTo(Role.Admin), "a weaker grant lower down doesn't downgrade");
        Assert.That(access.RoleOn(8), Is.EqualTo(Role.Admin));
    }

    [Test]
    public void Can_compares_against_the_minimum_role()
    {
        var access = Snapshot((2, Role.Manager));

        Assert.That(access.Can(4, Role.Viewer), Is.True);
        Assert.That(access.Can(4, Role.Manager), Is.True);
        Assert.That(access.Can(4, Role.Admin), Is.False);
        Assert.That(access.Can(6, Role.Viewer), Is.False);
    }

    [Test]
    public void UnitsWith_and_HasAnywhere_summarize_the_scope()
    {
        var access = Snapshot((2, Role.Viewer), (6, Role.Manager));

        Assert.That(access.UnitsWith(Role.Viewer), Is.EquivalentTo(new[] { 2, 4, 5, 6, 8 }));
        Assert.That(access.UnitsWith(Role.Manager), Is.EquivalentTo(new[] { 6, 8 }));
        Assert.That(access.HasAnywhere(Role.Manager), Is.True);
        Assert.That(access.HasAnywhere(Role.Admin), Is.False);
    }

    [Test]
    public void DefaultWriteUnit_is_the_shallowest_writable_unit()
    {
        Assert.That(Snapshot((6, Role.Manager), (2, Role.Admin)).DefaultWriteUnit, Is.EqualTo(2));
        Assert.That(Snapshot((8, Role.Manager), (4, Role.Manager)).DefaultWriteUnit, Is.EqualTo(4));
        Assert.That(Snapshot((1, Role.Viewer)).DefaultWriteUnit, Is.Null);
        Assert.That(Snapshot().DefaultWriteUnit, Is.Null);
    }
}
