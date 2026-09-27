using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Auth;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

public class AuthApiTests : ApiTestBase
{
    private static object Credentials(string username = SafetyOpsApiFactory.UserName, string password = SafetyOpsApiFactory.Password) =>
        new { username, password };

    [Test]
    public async Task Login_returns_the_user_and_sets_a_hardened_cookie()
    {
        using var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", Credentials(), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var user = await ReadAsync<CurrentUserDto>(response);
        Assert.That(user.UserName, Is.EqualTo(SafetyOpsApiFactory.UserName));
        Assert.That(user.DisplayName, Is.EqualTo("Test User"));

        var cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(AuthSetup.CookieName + "=", StringComparison.Ordinal));
        Assert.That(cookie, Does.Contain("httponly").IgnoreCase);
        Assert.That(cookie, Does.Contain("secure").IgnoreCase);
        Assert.That(cookie, Does.Contain("samesite=lax").IgnoreCase);
    }

    [Test]
    public async Task Login_username_is_case_insensitive()
    {
        using var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", Credentials(username: "TESTER"), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase(SafetyOpsApiFactory.UserName, "wrong-password")]
    [TestCase("nobody", SafetyOpsApiFactory.Password)]
    [TestCase(SafetyOpsApiFactory.UserName, "correct-horse-1")]
    public async Task Login_with_bad_credentials_returns_401_without_a_cookie(string username, string password)
    {
        using var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", Credentials(username, password), Json);

        var problem = await ReadProblemAsync(response, HttpStatusCode.Unauthorized);
        Assert.That(problem.Detail, Is.EqualTo("Invalid username or password."));
        Assert.That(response.Headers.Contains("Set-Cookie"), Is.False);
    }

    [Test]
    public async Task Login_requires_username_and_password()
    {
        using var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "", password = "" }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EquivalentTo(new[] { "username", "password" }));
    }

    [Test]
    public async Task Me_returns_the_signed_in_user()
    {
        var user = await Client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json);

        Assert.That(user!.UserName, Is.EqualTo(SafetyOpsApiFactory.UserName));
    }

    [Test]
    public async Task Me_without_a_cookie_returns_401_problem()
    {
        using var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/auth/me");

        await ReadProblemAsync(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_clears_the_session()
    {
        var logout = await Client.PostAsync("/api/auth/logout", null);
        var me = await Client.GetAsync("/api/auth/me");

        Assert.That(logout.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(me.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task A_cookie_for_a_deleted_account_stops_working()
    {
        await Factory.WithDbAsync(db => db.Users.Where(u => u.UserName == SafetyOpsApiFactory.UserName).ExecuteDeleteAsync());

        var response = await Client.GetAsync("/api/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Passwords_are_stored_as_verifiable_hashes_not_plain_text()
    {
        AppUser user = null!;
        await Factory.WithDbAsync(async db => user = await db.Users.SingleAsync(u => u.UserName == SafetyOpsApiFactory.UserName));

        Assert.That(user.PasswordHash, Is.Not.EqualTo(SafetyOpsApiFactory.Password));
        Assert.That(user.PasswordHash, Does.Not.Contain(SafetyOpsApiFactory.Password));
        Assert.That(new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, SafetyOpsApiFactory.Password),
            Is.EqualTo(PasswordVerificationResult.Success));
    }

    private static IEnumerable<TestCaseData> ProtectedEndpoints()
    {
        yield return new TestCaseData("GET", "/api/personnel");
        yield return new TestCaseData("GET", "/api/personnel/1");
        yield return new TestCaseData("POST", "/api/personnel");
        yield return new TestCaseData("PUT", "/api/personnel/1");
        yield return new TestCaseData("DELETE", "/api/personnel/1");
        yield return new TestCaseData("GET", "/api/training/classes");
        yield return new TestCaseData("GET", "/api/training/classes/1");
        yield return new TestCaseData("POST", "/api/training/classes");
        yield return new TestCaseData("PUT", "/api/training/classes/1");
        yield return new TestCaseData("DELETE", "/api/training/classes/1");
        yield return new TestCaseData("GET", "/api/training/courses");
        yield return new TestCaseData("GET", "/api/medical-surveillance/appointments");
        yield return new TestCaseData("GET", "/api/medical-surveillance/appointments/1");
        yield return new TestCaseData("POST", "/api/medical-surveillance/appointments");
        yield return new TestCaseData("PUT", "/api/medical-surveillance/appointments/1");
        yield return new TestCaseData("DELETE", "/api/medical-surveillance/appointments/1");
        yield return new TestCaseData("GET", "/api/medical-surveillance/persons");
        yield return new TestCaseData("GET", "/api/medical-surveillance/work-tasks");
    }

    [TestCaseSource(nameof(ProtectedEndpoints))]
    public async Task Every_module_endpoint_returns_401_when_signed_out(string method, string url)
    {
        using var client = CreateAnonymousClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(response.Headers.Location, Is.Null, "API must not redirect to a login page");
    }

    [Test]
    public async Task The_spa_shell_is_served_without_signing_in()
    {
        using var client = CreateAnonymousClient();

        var response = await client.GetAsync("/personnel");

        // No built SPA exists in the test output, so the fallback finds no file; the point is it isn't a 401.
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.Unauthorized));
    }
}
