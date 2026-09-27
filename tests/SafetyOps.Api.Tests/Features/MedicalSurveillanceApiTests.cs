using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.MedicalSurveillance;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Tests.Infrastructure;

namespace SafetyOps.Api.Tests.Features;

public class MedicalSurveillanceApiTests : ApiTestBase
{
    private static object Request(int personId, string date, params (string Id, string Exam)[] stressors) =>
        new { date, personId, stressors = stressors.Select(s => new { stressorId = s.Id, examType = s.Exam }) };

    [Test]
    public async Task CreateAppointment_returns_201_with_person_and_stressor_names()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];

        var response = await Client.PostAsJsonAsync("/api/medical-surveillance/appointments",
            Request(person.Id, "2026-06-01", ("STR-002", "Periodic"), ("STR-001", "Initial")), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await ReadAsync<AppointmentDto>(response);
        Assert.That(response.Headers.Location?.AbsolutePath, Is.EqualTo($"/api/medical-surveillance/appointments/{created.Id}"));
        Assert.That(created.PersonName, Is.EqualTo("Jane Doe"));
        Assert.That(created.Date, Is.EqualTo(new DateOnly(2026, 6, 1)));
        Assert.That(created.Stressors, Is.EqualTo(new[]
        {
            new AppointmentStressorDto("STR-001", "Solvent Exposure", "Initial"),
            new AppointmentStressorDto("STR-002", "Noise Exposure", "Periodic"),
        }));
    }

    [Test]
    public async Task CreateAppointment_rejects_unknown_person()
    {
        var response = await Client.PostAsJsonAsync("/api/medical-surveillance/appointments", Request(4242, "2026-06-01"), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["personId"], Is.EqualTo(new[] { "Unknown person 4242." }));
    }

    [Test]
    public async Task CreateAppointment_requires_date_and_person()
    {
        var response = await Client.PostAsJsonAsync("/api/medical-surveillance/appointments", new { personId = 0 }, Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors.Keys, Is.EquivalentTo(new[] { "date", "personId" }));
    }

    [Test]
    public async Task CreateAppointment_rejects_unknown_stressor()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];

        var response = await Client.PostAsJsonAsync("/api/medical-surveillance/appointments", Request(person.Id, "2026-06-01", ("STR-999", "Initial")), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["stressors"], Is.EqualTo(new[] { "Unknown stressor 'STR-999'." }));
    }

    [Test]
    public async Task CreateAppointment_rejects_the_same_stressor_twice()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];

        var response = await Client.PostAsJsonAsync("/api/medical-surveillance/appointments",
            Request(person.Id, "2026-06-01", ("STR-001", "Initial"), ("STR-001", "Exit")), Json);

        var problem = await ReadValidationProblemAsync(response);
        Assert.That(problem.Errors["stressors"], Is.EqualTo(new[] { "Stressor 'STR-001' is listed more than once." }));
        Assert.That(await CountAsync(db => db.MedicalAppointments), Is.Zero);
    }

    [Test]
    public async Task UpdateAppointment_replaces_date_person_and_stressors()
    {
        var people = await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("John", "Roe", "Ops"));
        var appointment = await SeedAppointmentAsync(people[0].Id, Today.AddDays(-3), ("STR-001", "Initial"), ("STR-002", "Initial"));

        var response = await Client.PutAsJsonAsync($"/api/medical-surveillance/appointments/{appointment.Id}",
            Request(people[1].Id, "2026-06-10", ("STR-002", "Exit"), ("STR-003", "Special")), Json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await Client.GetFromJsonAsync<AppointmentDto>($"/api/medical-surveillance/appointments/{appointment.Id}", Json);
        Assert.That(fetched!.PersonName, Is.EqualTo("John Roe"));
        Assert.That(fetched.Date, Is.EqualTo(new DateOnly(2026, 6, 10)));
        Assert.That(fetched.Stressors, Is.EqualTo(new[]
        {
            new AppointmentStressorDto("STR-002", "Noise Exposure", "Exit"),
            new AppointmentStressorDto("STR-003", "Dust Inhalation", "Special"),
        }));
    }

    [Test]
    public async Task UpdateAppointment_unknown_id_returns_404()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];

        var response = await Client.PutAsJsonAsync("/api/medical-surveillance/appointments/9999", Request(person.Id, "2026-06-01"), Json);

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateAppointment_with_invalid_body_returns_validation_problem()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];
        var appointment = await SeedAppointmentAsync(person.Id, Today, ("STR-001", "Initial"));

        var response = await Client.PutAsJsonAsync($"/api/medical-surveillance/appointments/{appointment.Id}", Request(person.Id, "06/01/2026"), Json);

        await ReadValidationProblemAsync(response);
    }

    [Test]
    public async Task GetAppointment_unknown_id_returns_404()
    {
        var response = await Client.GetAsync("/api/medical-surveillance/appointments/9999");

        var problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.That(problem.Detail, Is.EqualTo("Appointment not found."));
    }

    [Test]
    public async Task DeleteAppointment_returns_204_and_removes_its_stressor_rows()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];
        var appointment = await SeedAppointmentAsync(person.Id, Today, ("STR-001", "Initial"), ("STR-002", "Exit"));

        var response = await Client.DeleteAsync($"/api/medical-surveillance/appointments/{appointment.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await CountAsync(db => db.MedicalAppointments), Is.Zero);
        var stressorRows = 0;
        await Factory.WithDbAsync(async db =>
            stressorRows = await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AppointmentStressors").SingleAsync());
        Assert.That(stressorRows, Is.Zero);
    }

    [Test]
    public async Task DeleteAppointment_unknown_id_returns_404()
    {
        var response = await Client.DeleteAsync("/api/medical-surveillance/appointments/9999");

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [TestCase("doe", "Jane Doe")]
    [TestCase("06/14/2026", "John Roe")]
    [TestCase("2026-06-14", "John Roe")]
    public async Task ListAppointments_search_matches_name_or_date(string search, string expectedPerson)
    {
        var people = await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("John", "Roe", "Ops"));
        await SeedAppointmentAsync(people[0].Id, new DateOnly(2026, 6, 1));
        await SeedAppointmentAsync(people[1].Id, new DateOnly(2026, 6, 14));

        var page = await Client.GetFromJsonAsync<PagedResult<AppointmentDto>>($"/api/medical-surveillance/appointments?search={Uri.EscapeDataString(search)}", Json);

        Assert.That(page!.Items.Select(a => a.PersonName), Is.EqualTo(new[] { expectedPerson }));
    }

    [Test]
    public async Task ListAppointments_search_matches_id_and_no_longer_special_cases_the_word_appointments()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];
        await SeedAppointmentAsync(person.Id, Today);
        var second = await SeedAppointmentAsync(person.Id, Today);

        var byId = await Client.GetFromJsonAsync<PagedResult<AppointmentDto>>($"/api/medical-surveillance/appointments?search={second.Id}", Json);
        var byWord = await Client.GetFromJsonAsync<PagedResult<AppointmentDto>>("/api/medical-surveillance/appointments?search=appointments", Json);

        Assert.That(byId!.Items.Select(a => a.Id), Is.EqualTo(new[] { second.Id }));
        Assert.That(byWord!.Items, Is.Empty);
    }

    [Test]
    public async Task ListAppointments_returns_newest_first()
    {
        var person = (await SeedPeopleAsync(("Jane", "Doe", "Safety")))[0];
        var first = await SeedAppointmentAsync(person.Id, Today);
        var second = await SeedAppointmentAsync(person.Id, Today);

        var page = await Client.GetFromJsonAsync<PagedResult<AppointmentDto>>("/api/medical-surveillance/appointments", Json);

        Assert.That(page!.Items.Select(a => a.Id), Is.EqualTo(new[] { second.Id, first.Id }));
    }

    [Test]
    public async Task GetPersons_lists_people_by_full_name_with_search()
    {
        await SeedPeopleAsync(("Jane", "Doe", "Safety"), ("John", "Roe", "Ops"), ("Amy", "Doe", "Ops"));

        var all = await Client.GetFromJsonAsync<List<PersonOptionDto>>("/api/medical-surveillance/persons", Json);
        var filtered = await Client.GetFromJsonAsync<List<PersonOptionDto>>("/api/medical-surveillance/persons?search=jane%20d", Json);

        Assert.That(all!.Select(p => p.Name), Is.EqualTo(new[] { "Amy Doe", "Jane Doe", "John Roe" }));
        Assert.That(filtered!.Select(p => p.Name), Is.EqualTo(new[] { "Jane Doe" }));
    }

    [Test]
    public async Task GetWorkTasks_returns_stressors_and_exam_types_from_reference_data()
    {
        var tasks = (await Client.GetFromJsonAsync<List<WorkTaskDto>>("/api/medical-surveillance/work-tasks", Json))!;

        Assert.That(tasks.Select(t => t.Id), Is.EqualTo(new[] { "WT-001", "WT-002", "WT-003" }));
        Assert.That(tasks[0].Stressors, Is.EqualTo(new[] { new WorkTaskStressorDto("STR-001", "Solvent Exposure") }));
        Assert.That(tasks[0].ExamTypeOptions, Is.EqualTo(new[] { "Initial", "Periodic", "Exit", "Return to Duty" }));
    }
}
