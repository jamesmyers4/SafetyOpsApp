using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Training;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

public class TrainingApiTests : ApiTestBase
{
    private static object ValidRequest(string courseId = "PPE-001", string? classDate = null, string location = "Building 1 Room 2") =>
        new { courseId, classDate = classDate ?? Today.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), location };

    [Test]
    public async Task ListClasses_returns_newest_first_with_course_details()
    {
        var older = await SeedClassAsync("ELV-001", Today.AddDays(-10), "Room A");
        var newer = await SeedClassAsync("FAC-001", Today.AddDays(-20), "Room B");

        var page = await Client.GetFromJsonAsync<PagedResult<TrainingClassDto>>("/api/training/classes", Json);

        Assert.That(page!.Items, Is.EqualTo(new[]
        {
            new TrainingClassDto(newer.Id, "First Aid and CPR", "FAC-001", Today.AddDays(-20), "Room B", 1, "SafetyOps Industries"),
            new TrainingClassDto(older.Id, "Electrical - Low Voltage", "ELV-001", Today.AddDays(-10), "Room A", 1, "SafetyOps Industries"),
        }));
    }

    [TestCase("electrical", new[] { "Room A", "Room C" })]
    [TestCase("ROOM B", new[] { "Room B" })]
    public async Task ListClasses_search_matches_course_title_or_location(string search, string[] expectedLocations)
    {
        await SeedClassAsync("ELV-001", Today, "Room A");
        await SeedClassAsync("FAC-001", Today, "Room B");
        await SeedClassAsync("ELH-001", Today, "Room C");

        var page = await Client.GetFromJsonAsync<PagedResult<TrainingClassDto>>($"/api/training/classes?search={Uri.EscapeDataString(search)}", Json);

        Assert.That(page!.Items.Select(c => c.Location), Is.EquivalentTo(expectedLocations));
    }

    [Test]
    public async Task GetClass_unknown_id_returns_404()
    {
        var response = await Client.GetAsync("/api/training/classes/9999");

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateClass_returns_201_with_location_header_and_iso_date()
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", ValidRequest(classDate: "2026-06-01"), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await ReadAsync<TrainingClassDto>(response);
        Assert.That(response.Headers.Location?.AbsolutePath, Is.EqualTo($"/api/training/classes/{created.Id}"));
        Assert.That(created with { Id = 0 }, Is.EqualTo(new TrainingClassDto(0, "Personal Protective Equipment", "PPE-001", new DateOnly(2026, 6, 1), "Building 1 Room 2", 1, "SafetyOps Industries")));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("\"classDate\":\"2026-06-01\""));
    }

    [Test]
    public async Task CreateClass_allows_today()
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", ValidRequest(classDate: Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task CreateClass_rejects_future_dates_using_the_app_clock()
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", ValidRequest(classDate: Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["classDate"], Is.EqualTo(new[] { "Future dates are not allowed." }));
    }

    [Test]
    public async Task CreateClass_rejects_unknown_course()
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", ValidRequest(courseId: "NOPE-001"), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["courseId"], Is.EqualTo(new[] { "Unknown course 'NOPE-001'." }));
    }

    [TestCase("06/01/2026")]
    [TestCase("not a date")]
    public async Task CreateClass_rejects_non_iso_dates(string date)
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", ValidRequest(classDate: date), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Values.SelectMany(v => v), Does.Contain("Dates must be in yyyy-MM-dd format."));
    }

    [Test]
    public async Task CreateClass_requires_course_date_and_location()
    {
        var response = await Client.PostAsJsonAsync("/api/training/classes", new { }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EquivalentTo(new[] { "courseId", "classDate", "location" }));
    }

    [Test]
    public async Task UpdateClass_changes_course_date_and_location()
    {
        var cls = await SeedClassAsync("ELV-001", Today.AddDays(-10), "Room A");

        var response = await Client.PutAsJsonAsync($"/api/training/classes/{cls.Id}", ValidRequest("LOT-001", "2026-05-05", "Room Z"), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await Client.GetFromJsonAsync<TrainingClassDto>($"/api/training/classes/{cls.Id}", Json);
        Assert.That(fetched, Is.EqualTo(new TrainingClassDto(cls.Id, "Lockout/Tagout Procedures", "LOT-001", new DateOnly(2026, 5, 5), "Room Z", 1, "SafetyOps Industries")));
    }

    [Test]
    public async Task UpdateClass_unknown_id_returns_404()
    {
        var response = await Client.PutAsJsonAsync("/api/training/classes/9999", ValidRequest(), Json);

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateClass_with_invalid_body_returns_validation_problem()
    {
        var cls = await SeedClassAsync("ELV-001", Today.AddDays(-10), "Room A");

        var response = await Client.PutAsJsonAsync($"/api/training/classes/{cls.Id}", ValidRequest(location: ""), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EqualTo(new[] { "location" }));
    }

    [Test]
    public async Task DeleteClass_returns_204_then_404()
    {
        var cls = await SeedClassAsync("ELV-001", Today.AddDays(-10), "Room A");

        var first = await Client.DeleteAsync($"/api/training/classes/{cls.Id}");
        var second = await Client.DeleteAsync($"/api/training/classes/{cls.Id}");

        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        await ReadProblemAsync(second, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetCourses_returns_the_catalog_from_reference_data()
    {
        var courses = await Client.GetFromJsonAsync<List<CourseDto>>("/api/training/courses", Json);

        Assert.That(courses, Has.Count.EqualTo(8));
        Assert.That(courses![0], Is.EqualTo(new CourseDto("ELV-001", "Electrical - Low Voltage")));
    }

    [Test]
    public async Task GetCourses_search_filters_by_title()
    {
        var courses = await Client.GetFromJsonAsync<List<CourseDto>>("/api/training/courses?search=electrical", Json);

        Assert.That(courses!.Select(c => c.Id), Is.EqualTo(new[] { "ELV-001", "ELH-001", "ELS-001" }));
    }
}
