using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Access;
using SafetyOps.Api.Features.Auth;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Incidents;
using SafetyOps.Api.Features.MedicalSurveillance;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Features.Training;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

/// <summary>
/// Role × action × scope, against the seeded tree:
/// <code>
/// ORG (1)
/// ├── MFG (2) ── MFG-N (4), MFG-S (5)
/// └── LOG (3) ── LOG-E (6), LOG-W (7)
/// </code>
/// Viewer reads, Manager also writes, Admin also manages roles — each on the assigned unit and
/// everything below it, never above or beside it. Records outside read scope are 404; readable
/// but not writable is 403.
/// </summary>
public class AccessControlTests : ApiTestBase
{
    private const int Org = OrgUnit.RootId, Mfg = 2, Log = 3, North = 4, South = 5, East = 6, West = 7;
    private const string Password = "Passw0rd!";

    private readonly Dictionary<string, int> _userIds = [];
    private Person _north = null!, _south = null!, _east = null!, _west = null!, _mfg = null!;

    [SetUp]
    public async Task SeedTreeData()
    {
        await AddUserAsync("mfg-manager", (Mfg, Role.Manager));
        await AddUserAsync("north-manager", (North, Role.Manager));
        await AddUserAsync("north-viewer", (North, Role.Viewer));
        await AddUserAsync("log-admin", (Log, Role.Admin));
        await AddUserAsync("mixed", (Mfg, Role.Viewer), (North, Role.Manager));
        await AddUserAsync("nobody");

        _north = await SeedPersonAsync("Nora", "North", North);
        _south = await SeedPersonAsync("Sam", "South", South);
        _east = await SeedPersonAsync("Eve", "East", East);
        _west = await SeedPersonAsync("Wes", "West", West);
        _mfg = await SeedPersonAsync("Max", "Division", Mfg);
    }

    // ---------- Viewer ----------

    [Test]
    public async Task Viewer_sees_only_people_in_their_unit()
    {
        using var client = await SignInAsync("north-viewer");

        var page = await client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel", Json);

        Assert.That(page!.Items.Select(p => p.Id), Is.EqualTo(new[] { _north.Id }));
    }

    [Test]
    public async Task Viewer_gets_404_for_records_outside_their_scope_and_200_inside()
    {
        using var client = await SignInAsync("north-viewer");

        Assert.That((await client.GetAsync($"/api/personnel/{_north.Id}")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await client.GetAsync($"/api/personnel/{_south.Id}")).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That((await client.GetAsync($"/api/personnel/{_mfg.Id}")).StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "a role on a site doesn't reach its parent");
    }

    [Test]
    public async Task Viewer_cannot_create_update_or_delete()
    {
        using var client = await SignInAsync("north-viewer");

        var create = await client.PostAsJsonAsync("/api/personnel", Person("New", North), Json);
        var update = await client.PutAsJsonAsync($"/api/personnel/{_north.Id}", Person("Renamed"), Json);
        var delete = await client.DeleteAsync($"/api/personnel/{_north.Id}");

        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(await CountAsync(db => db.People), Is.EqualTo(5));
    }

    [Test]
    public async Task Viewer_cannot_manage_roles()
    {
        using var client = await SignInAsync("north-viewer");

        var list = await client.GetAsync("/api/access/assignments");
        var grant = await client.PostAsJsonAsync("/api/access/assignments", Grant("nobody", North, Role.Viewer), Json);

        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(grant.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // ---------- Manager ----------

    [Test]
    public async Task Division_manager_sees_the_division_and_its_sites_but_not_siblings_or_parent()
    {
        await SeedPersonAsync("Root", "Level", Org);
        using var client = await SignInAsync("mfg-manager");

        var page = await client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel?pageSize=100", Json);

        Assert.That(page!.Items.Select(p => p.Id), Is.EquivalentTo(new[] { _north.Id, _south.Id, _mfg.Id }));
    }

    [TestCase(South, HttpStatusCode.Created)]
    [TestCase(Mfg, HttpStatusCode.Created)]
    [TestCase(East, HttpStatusCode.Forbidden)]
    [TestCase(Org, HttpStatusCode.Forbidden)]
    public async Task Division_manager_can_create_only_inside_their_subtree(int unit, HttpStatusCode expected)
    {
        using var client = await SignInAsync("mfg-manager");

        var response = await client.PostAsJsonAsync("/api/personnel", Person("New", unit), Json);

        Assert.That(response.StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public async Task New_records_default_to_the_highest_unit_the_user_can_write()
    {
        using var client = await SignInAsync("mfg-manager");

        var response = await client.PostAsJsonAsync("/api/personnel", Person("Defaulted"), Json);

        var created = await ReadAsync<PersonDto>(response);
        Assert.That((created.OrgUnitId, created.OrgUnitName), Is.EqualTo((Mfg, "Manufacturing Division")));
    }

    [Test]
    public async Task Manager_can_move_records_within_scope_but_not_out_of_it()
    {
        using var client = await SignInAsync("mfg-manager");

        var within = await client.PutAsJsonAsync($"/api/personnel/{_north.Id}", Person("Nora", South), Json);
        var outside = await client.PutAsJsonAsync($"/api/personnel/{_north.Id}", Person("Nora", East), Json);

        Assert.That(within.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await ReadAsync<PersonDto>(within)).OrgUnitId, Is.EqualTo(South));
        Assert.That(outside.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Manager_gets_404_when_changing_records_outside_scope()
    {
        using var client = await SignInAsync("mfg-manager");

        var update = await client.PutAsJsonAsync($"/api/personnel/{_east.Id}", Person("Eve"), Json);
        var delete = await client.DeleteAsync($"/api/personnel/{_east.Id}");

        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Site_manager_cannot_write_at_the_division_level()
    {
        using var client = await SignInAsync("north-manager");

        var create = await client.PostAsJsonAsync("/api/personnel", Person("New", Mfg), Json);
        var update = await client.PutAsJsonAsync($"/api/personnel/{_mfg.Id}", Person("Max"), Json);

        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task The_strongest_role_on_the_path_wins()
    {
        // "mixed" is Viewer on MFG and Manager on MFG-N.
        using var client = await SignInAsync("mixed");

        var northEdit = await client.PutAsJsonAsync($"/api/personnel/{_north.Id}", Person("Nora"), Json);
        var southEdit = await client.PutAsJsonAsync($"/api/personnel/{_south.Id}", Person("Sam"), Json);

        Assert.That(northEdit.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(southEdit.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Manager_cannot_manage_roles()
    {
        using var client = await SignInAsync("mfg-manager");

        var grant = await client.PostAsJsonAsync("/api/access/assignments", Grant("nobody", North, Role.Viewer), Json);

        Assert.That(grant.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // ---------- Other modules follow the same scope ----------

    [Test]
    public async Task Training_classes_are_scoped_by_their_unit()
    {
        var northClass = await SeedClassAsync("ELV-001", Today.AddDays(-1), "North Room", North);
        var eastClass = await SeedClassAsync("ELV-001", Today.AddDays(-1), "East Room", East);
        using var viewer = await SignInAsync("north-viewer");
        using var manager = await SignInAsync("mfg-manager");

        var visible = await viewer.GetFromJsonAsync<PagedResult<TrainingClassDto>>("/api/training/classes", Json);
        var viewerDelete = await viewer.DeleteAsync($"/api/training/classes/{northClass.Id}");
        var managerEast = await manager.GetAsync($"/api/training/classes/{eastClass.Id}");
        var managerCreateEast = await manager.PostAsJsonAsync("/api/training/classes",
            new { courseId = "PPE-001", classDate = "2026-06-01", location = "x", orgUnitId = East }, Json);
        var courses = await viewer.GetAsync("/api/training/courses");

        Assert.That(visible!.Items.Select(c => c.Id), Is.EqualTo(new[] { northClass.Id }));
        Assert.That(viewerDelete.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(managerEast.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(managerCreateEast.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(courses.StatusCode, Is.EqualTo(HttpStatusCode.OK), "the course catalog is shared reference data");
    }

    [Test]
    public async Task Appointments_follow_the_unit_of_the_person_evaluated()
    {
        var northAppointment = await SeedAppointmentAsync(_north.Id, Today, ("STR-001", "Initial"));
        await SeedAppointmentAsync(_east.Id, Today, ("STR-001", "Initial"));
        using var manager = await SignInAsync("mfg-manager");
        using var viewer = await SignInAsync("north-viewer");

        var visible = await viewer.GetFromJsonAsync<PagedResult<AppointmentDto>>("/api/medical-surveillance/appointments", Json);
        var forSouth = await manager.PostAsJsonAsync("/api/medical-surveillance/appointments", new { date = "2026-06-01", personId = _south.Id }, Json);
        var forEast = await manager.PostAsJsonAsync("/api/medical-surveillance/appointments", new { date = "2026-06-01", personId = _east.Id }, Json);
        var viewerCreate = await viewer.PostAsJsonAsync("/api/medical-surveillance/appointments", new { date = "2026-06-01", personId = _north.Id }, Json);
        var persons = await viewer.GetFromJsonAsync<List<PersonOptionDto>>("/api/medical-surveillance/persons", Json);

        Assert.That(visible!.Items.Select(a => a.Id), Is.EqualTo(new[] { northAppointment.Id }));
        Assert.That(forSouth.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(forEast.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "people outside scope are unknown");
        Assert.That(viewerCreate.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(persons!.Select(p => p.Id), Is.EqualTo(new[] { _north.Id }));
    }

    [Test]
    public async Task Incidents_are_scoped_by_their_unit_and_reporters_must_be_visible()
    {
        using var manager = await SignInAsync("mfg-manager");
        object Incident(int reporter, int unit) => new
        {
            occurredAt = "2026-06-10T10:00",
            location = "Dock",
            category = "NearMiss",
            severity = "Low",
            description = "x",
            reportedById = reporter,
            orgUnitId = unit,
        };

        var inNorth = await manager.PostAsJsonAsync("/api/incidents", Incident(_south.Id, North), Json);
        var inEast = await manager.PostAsJsonAsync("/api/incidents", Incident(_north.Id, East), Json);
        var eastReporter = await manager.PostAsJsonAsync("/api/incidents", Incident(_east.Id, North), Json);
        using var viewer = await SignInAsync("north-viewer");
        var visible = await viewer.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents", Json);

        Assert.That(inNorth.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(inEast.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(eastReporter.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(visible!.Items.Select(i => i.OrgUnitName), Is.EqualTo(new[] { "North Plant" }));
    }

    // ---------- Admin ----------

    [Test]
    public async Task Admin_grants_roles_in_their_subtree_and_the_grant_takes_effect_immediately()
    {
        using var admin = await SignInAsync("log-admin");
        using var nobody = await SignInAsync("nobody");
        var before = await nobody.GetAsync("/api/personnel");

        var grant = await admin.PostAsJsonAsync("/api/access/assignments", Grant("nobody", East, Role.Viewer), Json);
        var after = await nobody.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel", Json);

        Assert.That(before.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(grant.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await ReadAsync<RoleAssignmentDto>(grant);
        Assert.That((created.UserName, created.OrgUnitName, created.Role), Is.EqualTo(("nobody", "East Warehouse", Role.Viewer)));
        Assert.That(after!.Items.Select(p => p.Id), Is.EqualTo(new[] { _east.Id }));
    }

    [TestCase(North)]
    [TestCase(Org)]
    public async Task Admin_cannot_grant_outside_their_subtree(int unit)
    {
        using var admin = await SignInAsync("log-admin");

        var response = await admin.PostAsJsonAsync("/api/access/assignments", Grant("nobody", unit, Role.Viewer), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Admin_lists_only_assignments_in_their_subtree()
    {
        using var admin = await SignInAsync("log-admin");

        var list = await admin.GetFromJsonAsync<List<RoleAssignmentDto>>("/api/access/assignments", Json);

        Assert.That(list!.Select(a => a.UserName), Is.EqualTo(new[] { "log-admin" }));
    }

    [Test]
    public async Task Granting_a_second_role_on_the_same_unit_is_a_conflict()
    {
        using var admin = await SignInAsync("log-admin");
        await admin.PostAsJsonAsync("/api/access/assignments", Grant("nobody", West, Role.Viewer), Json);

        var again = await admin.PostAsJsonAsync("/api/access/assignments", Grant("nobody", West, Role.Manager), Json);

        await ReadProblemAsync(again, HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Revoking_removes_access_on_the_next_request()
    {
        using var admin = await SignInAsync("log-admin");
        var granted = await ReadAsync<RoleAssignmentDto>(
            await admin.PostAsJsonAsync("/api/access/assignments", Grant("nobody", East, Role.Manager), Json));
        using var nobody = await SignInAsync("nobody");
        Assert.That((await nobody.GetAsync("/api/personnel")).StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var revoke = await admin.DeleteAsync($"/api/access/assignments/{granted.Id}");

        Assert.That(revoke.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await nobody.GetAsync("/api/personnel")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Admin_cannot_revoke_their_own_role_or_roles_outside_their_subtree()
    {
        using var admin = await SignInAsync("log-admin");
        var assignments = await Client.GetFromJsonAsync<List<RoleAssignmentDto>>("/api/access/assignments", Json);
        var own = assignments!.Single(a => a.UserName == "log-admin");
        var outside = assignments!.Single(a => a.UserName == "mfg-manager");

        var revokeOwn = await admin.DeleteAsync($"/api/access/assignments/{own.Id}");
        var revokeOutside = await admin.DeleteAsync($"/api/access/assignments/{outside.Id}");

        await ReadProblemAsync(revokeOwn, HttpStatusCode.Conflict);
        await ReadProblemAsync(revokeOutside, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Grant_validates_user_unit_and_role()
    {
        var unknownUser = await Client.PostAsJsonAsync("/api/access/assignments", new { userId = 999, orgUnitId = East, role = "Viewer" }, Json);
        var unknownUnit = await Client.PostAsJsonAsync("/api/access/assignments", new { userId = _userIds["nobody"], orgUnitId = 999, role = "Viewer" }, Json);
        var badRole = await Client.PostAsJsonAsync("/api/access/assignments", new { userId = _userIds["nobody"], orgUnitId = East, role = "Owner" }, Json);

        Assert.That((await ReadValidationProblemAsync(unknownUser)).Errors.Keys, Is.EqualTo(new[] { "userId" }));
        Assert.That((await ReadValidationProblemAsync(unknownUnit)).Errors.Keys, Is.EqualTo(new[] { "orgUnitId" }));
        await ReadValidationProblemAsync(badRole);
    }

    // ---------- No role / summary ----------

    [Test]
    public async Task A_user_with_no_role_can_sign_in_but_every_module_endpoint_is_forbidden()
    {
        using var client = await SignInAsync("nobody");

        var me = await client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json);

        Assert.That(me!.Access, Is.EqualTo(new AccessSummaryDto(false, false, false, [])).Using<AccessSummaryDto>(SameSummary));
        foreach (var url in new[] { "/api/personnel", "/api/training/classes", "/api/training/courses", "/api/medical-surveillance/appointments", "/api/incidents", "/api/access/org-units" })
            Assert.That((await client.GetAsync(url)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), url);
    }

    [Test]
    public async Task Me_and_org_units_describe_the_callers_access()
    {
        using var client = await SignInAsync("mfg-manager");

        var me = await client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json);
        var units = await client.GetFromJsonAsync<List<OrgUnitDto>>("/api/access/org-units", Json);

        Assert.That((me!.Access.CanRead, me.Access.CanWrite, me.Access.CanManageRoles), Is.EqualTo((true, true, false)));
        Assert.That(me.Access.Grants, Is.EqualTo(new[] { new AccessGrantDto(Mfg, "Manufacturing Division", Role.Manager) }));
        Assert.That(units!.Select(u => (u.Code, u.MyRole)), Is.EqualTo(new[] { ("MFG", Role.Manager), ("MFG-N", Role.Manager), ("MFG-S", Role.Manager) }));
    }

    [Test]
    public async Task Root_admin_sees_every_unit()
    {
        var units = (await Client.GetFromJsonAsync<List<OrgUnitDto>>("/api/access/org-units", Json))!;

        Assert.That(units.Select(u => u.Id), Is.EqualTo(new[] { Org, Mfg, Log, North, South, East, West }));
        Assert.That(units.All(u => u.MyRole == Role.Admin));
    }

    // ---------- helpers ----------

    private static bool SameSummary(AccessSummaryDto a, AccessSummaryDto b) =>
        a.CanRead == b.CanRead && a.CanWrite == b.CanWrite && a.CanManageRoles == b.CanManageRoles && a.Grants.SequenceEqual(b.Grants);

    private static object Person(string first, int? orgUnitId = null) => new { firstName = first, lastName = "Test", orgUnitId };

    private object Grant(string user, int orgUnitId, Role role) => new { userId = _userIds[user], orgUnitId, role };

    private async Task AddUserAsync(string userName, params (int Unit, Role Role)[] roles)
    {
        await Factory.WithDbAsync(async db =>
        {
            var user = new AppUser { UserName = userName, DisplayName = userName };
            user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, Password);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RoleAssignments.AddRange(roles.Select(r => new RoleAssignment { UserId = user.Id, OrgUnitId = r.Unit, Role = r.Role }));
            await db.SaveChangesAsync();
            _userIds[userName] = user.Id;
        });
    }

    private async Task<Person> SeedPersonAsync(string first, string last, int unit)
    {
        var person = new Person { FirstName = first, LastName = last, OrgUnitId = unit };
        await Factory.WithDbAsync(async db =>
        {
            db.People.Add(person);
            await db.SaveChangesAsync();
        });
        return person;
    }

    private async Task<HttpClient> SignInAsync(string userName)
    {
        var client = CreateAnonymousClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = userName, password = Password }, Json);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"sign-in as {userName}");
        return client;
    }
}
