using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using SafetyOps.Api.Data;

namespace SafetyOps.Api.Tests.Infrastructure;

/// <summary>
/// Hosts the API in memory against its own in-memory SQLite database. The connection stays
/// open for the factory's lifetime (SQLite drops an in-memory database when its last connection
/// closes), so every factory, and therefore every test, gets a fresh, fully migrated database.
/// </summary>
public sealed class SafetyOpsApiFactory : WebApplicationFactory<Program>
{
    /// <summary>"Today" for every test: 2026-06-15 (UTC).</summary>
    public static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    public const string UserName = "tester";
    public const string Password = "Correct-Horse-1";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SafetyOpsApiFactory()
    {
        _connection.Open();
        // The auth cookie is Secure-only, so the test client has to speak HTTPS for it to be sent back.
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    public FakeTimeProvider Clock { get; } = new(Now);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development, so the demo data seeder stays off and each test starts with reference data only.
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.UseSetting("Auth:DemoUsers:0:UserName", UserName);
        builder.UseSetting("Auth:DemoUsers:0:Password", Password);
        builder.UseSetting("Auth:DemoUsers:0:DisplayName", "Test User");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    /// <summary>Runs <paramref name="action"/> against the test database in its own scope.</summary>
    public async Task WithDbAsync(Func<AppDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
