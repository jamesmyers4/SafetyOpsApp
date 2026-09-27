using System.Net;
using System.Net.Http.Json;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Incidents;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

public class IncidentsApiTests : ApiTestBase
{
    private int _reporterId;

    [SetUp]
    public async Task SeedReporter() => _reporterId = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0].Id;

    private object Request(
        string occurredAt = "2026-06-14T09:30",
        string location = "Building 1 Dock",
        string category = "NearMiss",
        string severity = "Medium",
        string description = "Forklift reversed without a spotter.",
        int? reportedById = null,
        string? status = null) =>
        new { occurredAt, location, category, severity, description, reportedById = reportedById ?? _reporterId, status };

    private async Task<IncidentDto> CreateAsync(object request)
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", request, Json);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());
        return await ReadAsync<IncidentDto>(response);
    }

    [Test]
    public async Task Create_returns_201_defaults_status_to_open_and_uses_string_enums()
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", Request(), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await ReadAsync<IncidentDto>(response);
        Assert.That(response.Headers.Location?.AbsolutePath, Is.EqualTo($"/api/incidents/{created.Id}"));
        Assert.That(created with { Id = 0 }, Is.EqualTo(new IncidentDto(
            0, new DateTime(2026, 6, 14, 9, 30, 0), "Building 1 Dock", IncidentCategory.NearMiss, IncidentSeverity.Medium,
            "Forklift reversed without a spotter.", _reporterId, "Jane Doe", IncidentStatus.Open, 1, "SafetyOps Industries")));
        var json = await response.Content.ReadAsStringAsync();
        Assert.That(json, Does.Contain("\"category\":\"NearMiss\"").And.Contain("\"status\":\"Open\"").And.Contain("\"occurredAt\":\"2026-06-14T09:30:00\""));
    }

    [Test]
    public async Task Create_requires_every_field()
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", new { }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EquivalentTo(new[] { "occurredAt", "location", "category", "severity", "description", "reportedById" }));
    }

    [Test]
    public async Task Create_rejects_unknown_enum_values()
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", Request(category: "Explosion"), Json);

        await ReadValidationProblemAsync(response);
    }

    [Test]
    public async Task Create_rejects_a_future_time_using_the_app_clock()
    {
        // The factory's clock reads 2026-06-15 12:00.
        var response = await Client.PostAsJsonAsync("/api/incidents", Request(occurredAt: "2026-06-15T12:01"), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["occurredAt"], Is.EqualTo(new[] { "The incident can't be in the future." }));
    }

    [Test]
    public async Task Create_rejects_unknown_reporter()
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", Request(reportedById: 999), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["reportedById"], Is.EqualTo(new[] { "Unknown person 999." }));
    }

    [Test]
    public async Task Create_rejects_overlong_description()
    {
        var response = await Client.PostAsJsonAsync("/api/incidents", Request(description: new string('x', 2001)), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EqualTo(new[] { "description" }));
    }

    [Test]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await Client.GetAsync("/api/incidents/9999");

        var problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.That(problem.Detail, Is.EqualTo("Incident not found."));
    }

    [Test]
    public async Task Update_changes_details_and_status()
    {
        var created = await CreateAsync(Request());

        var response = await Client.PutAsJsonAsync($"/api/incidents/{created.Id}",
            Request(severity: "High", description: "Updated.", status: "UnderReview"), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await Client.GetFromJsonAsync<IncidentDto>($"/api/incidents/{created.Id}", Json);
        Assert.That(fetched!.Severity, Is.EqualTo(IncidentSeverity.High));
        Assert.That(fetched.Description, Is.EqualTo("Updated."));
        Assert.That(fetched.Status, Is.EqualTo(IncidentStatus.UnderReview));
    }

    [Test]
    public async Task Update_without_status_keeps_the_current_status()
    {
        var created = await CreateAsync(Request(status: "Closed"));

        await Client.PutAsJsonAsync($"/api/incidents/{created.Id}", Request(description: "Edited"), Json);

        var fetched = await Client.GetFromJsonAsync<IncidentDto>($"/api/incidents/{created.Id}", Json);
        Assert.That(fetched!.Status, Is.EqualTo(IncidentStatus.Closed));
    }

    [Test]
    public async Task Update_unknown_id_returns_404()
    {
        var response = await Client.PutAsJsonAsync("/api/incidents/9999", Request(), Json);

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Update_with_invalid_body_returns_validation_problem()
    {
        var created = await CreateAsync(Request());

        var response = await Client.PutAsJsonAsync($"/api/incidents/{created.Id}", Request(location: ""), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EqualTo(new[] { "location" }));
    }

    [Test]
    public async Task Delete_returns_204_then_404()
    {
        var created = await CreateAsync(Request());

        var first = await Client.DeleteAsync($"/api/incidents/{created.Id}");
        var second = await Client.DeleteAsync($"/api/incidents/{created.Id}");

        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        await ReadProblemAsync(second, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_is_most_recent_first_and_filters_by_status_category_and_search()
    {
        var older = await CreateAsync(Request(occurredAt: "2026-06-01T08:00", category: "Fire", description: "Bin fire", status: "Closed"));
        var newer = await CreateAsync(Request(occurredAt: "2026-06-10T08:00", category: "Injury", location: "Paint Shop"));

        var all = await Client.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents", Json);
        var closed = await Client.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents?status=Closed", Json);
        var injuries = await Client.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents?category=Injury", Json);
        var byLocation = await Client.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents?search=paint", Json);
        var byReporter = await Client.GetFromJsonAsync<PagedResult<IncidentDto>>("/api/incidents?search=jane%20doe", Json);

        Assert.That(all!.Items.Select(i => i.Id), Is.EqualTo(new[] { newer.Id, older.Id }));
        Assert.That(closed!.Items.Select(i => i.Id), Is.EqualTo(new[] { older.Id }));
        Assert.That(injuries!.Items.Select(i => i.Id), Is.EqualTo(new[] { newer.Id }));
        Assert.That(byLocation!.Items.Select(i => i.Id), Is.EqualTo(new[] { newer.Id }));
        Assert.That(byReporter!.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task List_rejects_an_unknown_status_filter()
    {
        var response = await Client.GetAsync("/api/incidents?status=Lost");

        await ReadValidationProblemAsync(response);
    }

    [Test]
    public async Task Deleting_a_person_who_reported_an_incident_returns_409()
    {
        await CreateAsync(Request());

        var response = await Client.DeleteAsync($"/api/personnel/{_reporterId}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.That(problem.Detail, Does.Contain("incidents"));
    }

    [Test]
    public async Task Personnel_lookup_lists_people_as_options()
    {
        await SeedPeopleAsync(("Amy", "Zed", "Ops"));

        var options = await Client.GetFromJsonAsync<List<PersonOptionDto>>("/api/personnel/lookup?search=doe", Json);

        Assert.That(options, Is.EqualTo(new[] { new PersonOptionDto(_reporterId, "Jane Doe") }));
    }
}
