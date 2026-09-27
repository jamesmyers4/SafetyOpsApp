using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using SafetyOps.Api.Data;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Training;

namespace SafetyOps.Api.Tests.Unit;

/// <summary>Services and the seeder exercised directly against a migrated in-memory SQLite database, without HTTP.</summary>
public class ServiceTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;
    private FakeTimeProvider _clock = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        await _db.Database.MigrateAsync();
        _clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 23, 30, 0, TimeSpan.Zero));
    }

    [TearDown]
    public async Task TearDown()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task Training_future_date_rule_follows_the_clock()
    {
        var service = new TrainingService(_db, _clock);
        var request = new TrainingClassRequest { CourseId = "ELV-001", ClassDate = new DateOnly(2026, 6, 16), Location = "Room 1" };

        var tooEarly = await service.CreateClassAsync(request);
        _clock.Advance(TimeSpan.FromHours(1)); // now 2026-06-16 00:30
        var onTheDay = await service.CreateClassAsync(request);

        Assert.That(tooEarly.Error, Is.EqualTo(Error.Invalid("classDate", "Future dates are not allowed.")));
        Assert.That(onTheDay.Error, Is.Null);
        Assert.That(onTheDay.Value!.ClassDate, Is.EqualTo(new DateOnly(2026, 6, 16)));
    }

    [Test]
    public async Task Training_update_trims_location_and_switches_course()
    {
        var service = new TrainingService(_db, _clock);
        var created = (await service.CreateClassAsync(new() { CourseId = "ELV-001", ClassDate = new DateOnly(2026, 6, 1), Location = "Room 1" })).Value!;

        var updated = await service.UpdateClassAsync(created.Id, new() { CourseId = "FAC-001", ClassDate = new DateOnly(2026, 6, 2), Location = "  Room 2  " });

        Assert.That(updated.Value, Is.EqualTo(new TrainingClassDto(created.Id, "First Aid and CPR", "FAC-001", new DateOnly(2026, 6, 2), "Room 2")));
    }

    [Test]
    public async Task Demo_seeder_fills_an_empty_database_relative_to_today_and_is_idempotent()
    {
        await DemoDataSeeder.SeedAsync(_db, _clock);
        await DemoDataSeeder.SeedAsync(_db, _clock);

        Assert.That(await _db.People.CountAsync(), Is.EqualTo(10));
        Assert.That(await _db.TrainingClasses.CountAsync(), Is.EqualTo(5));
        Assert.That(await _db.MedicalAppointments.CountAsync(), Is.EqualTo(3));
        var newestClass = await _db.TrainingClasses.MaxAsync(c => c.ClassDate);
        Assert.That(newestClass, Is.EqualTo(new DateOnly(2026, 6, 10)));
    }
}
