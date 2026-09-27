using System.Net;
using System.Net.Http.Json;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

public class PersonnelApiTests : ApiTestBase
{
    private static PersonRequest ValidRequest(string first = "Pat", string last = "Example") => new()
    {
        FirstName = first,
        LastName = last,
        MiddleName = "Q",
        Gender = "Female",
        Department = "Safety",
        EmployeeCategory = "Full Time",
        Subscription = "Basic",
        EmployeeNumber = "1234567",
    };

    [Test]
    public async Task List_returns_people_sorted_by_last_then_first_name_in_a_paged_envelope()
    {
        await SeedPeopleAsync(("Zoe", "Adams", "Ops"), ("Amy", "Baker", "Ops"), ("Adam", "Adams", "Ops"));

        var page = await Client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel", Json);

        Assert.That(page!.Items.Select(p => $"{p.FirstName} {p.LastName}"), Is.EqualTo(new[] { "Adam Adams", "Zoe Adams", "Amy Baker" }));
        Assert.That(page.TotalCount, Is.EqualTo(3));
        Assert.That(page.Page, Is.EqualTo(1));
        Assert.That(page.PageSize, Is.EqualTo(25));
    }

    [Test]
    public async Task List_search_is_case_insensitive_and_matches_name_or_department()
    {
        await SeedPeopleAsync(("John", "Smith", "Engineering"), ("Jane", "Doe", "Safety"), ("Carl", "Jones", "Finance"));

        var byName = await Client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel?search=SMI", Json);
        var byDepartment = await Client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel?search=safety", Json);

        Assert.That(byName!.Items.Select(p => p.LastName), Is.EqualTo(new[] { "Smith" }));
        Assert.That(byDepartment!.Items.Select(p => p.LastName), Is.EqualTo(new[] { "Doe" }));
    }

    [Test]
    public async Task List_search_treats_like_wildcards_literally()
    {
        await SeedPeopleAsync(("Ann", "Lee", "R&D"), ("Bob", "Ray", "100% Safety"));

        var page = await Client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel?search=%25", Json);

        Assert.That(page!.Items.Select(p => p.FirstName), Is.EqualTo(new[] { "Bob" }));
    }

    [Test]
    public async Task List_pages_through_results()
    {
        await SeedPeopleAsync(("A", "One", "x"), ("B", "Two", "x"), ("C", "Three", "x"), ("D", "Four", "x"), ("E", "Five", "x"));

        var page = await Client.GetFromJsonAsync<PagedResult<PersonDto>>("/api/personnel?page=2&pageSize=2", Json);

        Assert.That(page!.Items.Select(p => p.LastName), Is.EqualTo(new[] { "One", "Three" }));
        Assert.That(page.TotalCount, Is.EqualTo(5));
        Assert.That(page.TotalPages, Is.EqualTo(3));
    }

    [TestCase("pageSize=0")]
    [TestCase("pageSize=101")]
    [TestCase("page=0")]
    public async Task List_rejects_out_of_range_paging(string query)
    {
        var response = await Client.GetAsync($"/api/personnel?{query}");

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors, Is.Not.Empty);
    }

    [Test]
    public async Task Get_returns_the_person()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("John", "Roe", "Ops")))[0];

        var dto = await Client.GetFromJsonAsync<PersonDto>($"/api/personnel/{person.Id}", Json);

        Assert.That(dto, Is.EqualTo(new PersonDto(person.Id, "Jane", "Doe", "", "", "Safety", "", "", "")));
    }

    [Test]
    public async Task Get_unknown_id_returns_404_problem()
    {
        var response = await Client.GetAsync("/api/personnel/9999");

        var problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.That(problem.Detail, Is.EqualTo("Person not found."));
    }

    [Test]
    public async Task Create_returns_201_with_location_and_persists()
    {
        var response = await Client.PostAsJsonAsync("/api/personnel", ValidRequest(), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await ReadAsync<PersonDto>(response);
        Assert.That(response.Headers.Location?.AbsolutePath, Is.EqualTo($"/api/personnel/{created.Id}"));
        Assert.That(created with { Id = 0 }, Is.EqualTo(new PersonDto(0, "Pat", "Example", "Q", "Female", "Safety", "Full Time", "Basic", "1234567")));

        var fetched = await Client.GetFromJsonAsync<PersonDto>(response.Headers.Location, Json);
        Assert.That(fetched, Is.EqualTo(created));
    }

    [Test]
    public async Task Create_ignores_an_id_in_the_body()
    {
        var response = await Client.PostAsJsonAsync("/api/personnel", new { id = 777, firstName = "Pat", lastName = "Example" }, Json);

        var created = await ReadAsync<PersonDto>(response);
        Assert.That(created.Id, Is.Not.EqualTo(777));
    }

    [Test]
    public async Task Create_with_missing_names_and_bad_employee_number_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync("/api/personnel", ValidRequest() with { FirstName = "", LastName = " ", EmployeeNumber = "12a" }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EquivalentTo(new[] { "firstName", "lastName", "employeeNumber" }));
        Assert.That(await CountAsync(db => db.People), Is.Zero);
    }

    [Test]
    public async Task Create_with_too_long_name_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync("/api/personnel", ValidRequest(first: new string('x', 101)), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EqualTo(new[] { "firstName" }));
    }

    [Test]
    public async Task Create_with_empty_body_returns_validation_problem()
    {
        var response = await Client.PostAsync("/api/personnel", new StringContent("", null, "application/json"));

        await ReadValidationProblemAsync(response);
    }

    [Test]
    public async Task Update_replaces_the_details()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("x", "y", "z")))[0];

        var response = await Client.PutAsJsonAsync($"/api/personnel/{person.Id}", ValidRequest("Janet", "Doe-Smith"), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await ReadAsync<PersonDto>(response);
        Assert.That(updated.Id, Is.EqualTo(person.Id));
        Assert.That(updated.FirstName, Is.EqualTo("Janet"));
        var fetched = await Client.GetFromJsonAsync<PersonDto>($"/api/personnel/{person.Id}", Json);
        Assert.That(fetched, Is.EqualTo(updated));
    }

    [Test]
    public async Task Update_unknown_id_returns_404()
    {
        var response = await Client.PutAsJsonAsync("/api/personnel/9999", ValidRequest(), Json);

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Update_with_invalid_body_returns_validation_problem()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("x", "y", "z")))[0];

        var response = await Client.PutAsJsonAsync($"/api/personnel/{person.Id}", ValidRequest() with { LastName = "" }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EqualTo(new[] { "lastName" }));
    }

    [Test]
    public async Task Delete_returns_204_and_removes_the_person()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("x", "y", "z")))[0];

        var response = await Client.DeleteAsync($"/api/personnel/{person.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var again = await Client.GetAsync($"/api/personnel/{person.Id}");
        Assert.That(again.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_unknown_id_returns_404()
    {
        var response = await Client.DeleteAsync("/api/personnel/9999");

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_person_with_appointments_returns_409_and_keeps_them()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("x", "y", "z")))[0];
        await SeedAppointmentAsync(person.Id, Today.AddDays(-1), ("STR-001", "Initial"));

        var response = await Client.DeleteAsync($"/api/personnel/{person.Id}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.That(problem.Detail, Does.Contain("appointments"));
        Assert.That(await CountAsync(db => db.People), Is.EqualTo(2));
    }
}
