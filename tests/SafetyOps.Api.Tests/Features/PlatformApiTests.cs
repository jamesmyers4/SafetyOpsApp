using System.Net;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

/// <summary>Cross-cutting behavior: routing fallbacks and error format.</summary>
public class PlatformApiTests : ApiTestBase
{
    [Test]
    public async Task Unknown_api_route_returns_404_problem_instead_of_the_spa()
    {
        var response = await Client.GetAsync("/api/does-not-exist");

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Non_numeric_id_does_not_match_the_route()
    {
        var response = await Client.GetAsync("/api/personnel/abc");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Demo_data_is_not_seeded_outside_development()
    {
        Assert.That(await CountAsync(db => db.People), Is.Zero);
        Assert.That(await CountAsync(db => db.Courses), Is.EqualTo(8));
    }
}
