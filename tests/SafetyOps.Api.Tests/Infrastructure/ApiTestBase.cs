using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Tests.Infrastructure;

/// <summary>Gives each test its own app instance, database, and HTTP client.</summary>
public abstract class ApiTestBase
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected SafetyOpsApiFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void CreateApp()
    {
        Factory = new SafetyOpsApiFactory();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void DisposeApp()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected static DateOnly Today => DateOnly.FromDateTime(SafetyOpsApiFactory.Now.UtcDateTime);

    protected async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    protected async Task<ValidationProblemDetails> ReadValidationProblemAsync(HttpResponseMessage response)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json))!;
    }

    protected async Task<ProblemDetails> ReadProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        return (await response.Content.ReadFromJsonAsync<ProblemDetails>(Json))!;
    }

    /// <summary>Inserts people directly and returns them with their generated ids.</summary>
    protected async Task<Person[]> SeedPeopleAsync(params (string First, string Last, string Department)[] people)
    {
        var entities = people.Select(p => new Person { FirstName = p.First, LastName = p.Last, Department = p.Department }).ToArray();
        await Factory.WithDbAsync(async db =>
        {
            db.People.AddRange(entities);
            await db.SaveChangesAsync();
        });
        return entities;
    }

    protected async Task<TrainingClass> SeedClassAsync(string courseCode, DateOnly date, string location)
    {
        TrainingClass cls = null!;
        await Factory.WithDbAsync(async db =>
        {
            var course = db.Courses.Single(c => c.Code == courseCode);
            cls = new TrainingClass { CourseId = course.Id, ClassDate = date, Location = location };
            db.TrainingClasses.Add(cls);
            await db.SaveChangesAsync();
        });
        return cls;
    }

    protected async Task<MedicalAppointment> SeedAppointmentAsync(int personId, DateOnly date, params (string StressorCode, string ExamType)[] stressors)
    {
        MedicalAppointment appointment = null!;
        await Factory.WithDbAsync(async db =>
        {
            var ids = db.Stressors.ToDictionary(s => s.Code, s => s.Id);
            appointment = new MedicalAppointment
            {
                PersonId = personId,
                Date = date,
                Stressors = stressors.Select(s => new AppointmentStressor { StressorId = ids[s.StressorCode], ExamType = s.ExamType }).ToList(),
            };
            db.MedicalAppointments.Add(appointment);
            await db.SaveChangesAsync();
        });
        return appointment;
    }

    protected async Task<int> CountAsync<T>(Func<AppDbContext, IQueryable<T>> set)
    {
        var count = 0;
        await Factory.WithDbAsync(async db => count = await set(db).CountAsync());
        return count;
    }
}
