using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data;

/// <summary>
/// Seeds placeholder people, classes, and appointments into an empty database so the
/// app has something to show. Reference data (courses, stressors, work tasks) comes
/// from the migrations instead. Safe to run on every startup.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
    {
        if (await db.People.AnyAsync(ct))
            return;

        // Sites from the seeded org tree: Manufacturing (North/South Plant), Logistics (East/West Warehouse).
        var sites = await db.OrgUnits.ToDictionaryAsync(u => u.Code, u => u.Id, ct);
        int north = sites["MFG-N"], south = sites["MFG-S"], east = sites["LOG-E"], west = sites["LOG-W"];

        Person P(string first, string last, string middle, string gender, string dept, string category, string subscription, string number, int site) =>
            new() { FirstName = first, LastName = last, MiddleName = middle, Gender = gender, Department = dept, EmployeeCategory = category, Subscription = subscription, EmployeeNumber = number, OrgUnitId = site };

        var people = new[]
        {
            P("John",   "Smith",    "A", "Male",   "Engineering",     "Full Time",  "Standard", "1000001", north),
            P("Jane",   "Smith",    "B", "Female", "Operations",      "Full Time",  "Basic",    "1000002", north),
            P("Robert", "Smith",    "C", "Male",   "Safety",          "Full Time",  "Premium",  "1000003", north),
            P("Mary",   "Smith",    "D", "Female", "Finance",         "Contractor", "Standard", "1000004", south),
            P("John",   "Doe",      "E", "Male",   "Finance",         "Full Time",  "Standard", "1000005", south),
            P("John",   "Johnson",  "F", "Male",   "Human Resources", "Part Time",  "Basic",    "1000006", east),
            P("John",   "Williams", "G", "Male",   "Engineering",     "Full Time",  "Premium",  "1000007", east),
            P("Alice",  "Anderson", "H", "Female", "Operations",      "Full Time",  "Premium",  "1000008", west),
            P("Carol",  "Davis",    "I", "Female", "Finance",         "Part Time",  "Standard", "1000009", west),
            P("David",  "Wilson",   "J", "Male",   "Human Resources", "Full Time",  "Premium",  "1000010", west),
        };
        db.People.AddRange(people);

        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        var courses = await db.Courses.ToDictionaryAsync(c => c.Code, ct);
        TrainingClass C(string code, int daysAgo, string location, int site) =>
            new() { Course = courses[code], ClassDate = today.AddDays(-daysAgo), Location = location, OrgUnitId = site };

        db.TrainingClasses.AddRange(
            C("ELV-001", 10, "Building 100 Room 101", north),
            C("ELV-001", 5, "Building 200 Room 202", north),
            C("ELV-001", 20, "Building 300 Room 303", south),
            C("ELH-001", 15, "Building 400 Room 404", east),
            C("ELS-001", 25, "Building 100 Room 105", west));

        var stressors = await db.Stressors.ToDictionaryAsync(s => s.Code, ct);
        MedicalAppointment A(Person person, int daysAgo, string stressorCode, string examType) => new()
        {
            Person = person,
            Date = today.AddDays(-daysAgo),
            Stressors = [new AppointmentStressor { Stressor = stressors[stressorCode], ExamType = examType }],
        };

        db.MedicalAppointments.AddRange(
            A(people[0], 3, "STR-001", "Initial"),
            A(people[1], 7, "STR-002", "Periodic"),
            A(people[2], 14, "STR-003", "Exit"));

        var now = clock.GetLocalNow().DateTime;
        Incident I(int daysAgo, int hour, string location, IncidentCategory category, IncidentSeverity severity, string description, Person reporter, IncidentStatus status, int site) => new()
        {
            OrgUnitId = site,
            OccurredAt = now.Date.AddDays(-daysAgo).AddHours(hour),
            Location = location,
            Category = category,
            Severity = severity,
            Description = description,
            ReportedBy = reporter,
            Status = status,
        };

        db.Incidents.AddRange(
            I(2, 10, "Building 200 Loading Dock", IncidentCategory.NearMiss, IncidentSeverity.Medium,
                "Forklift reversed without a spotter; pedestrian stepped clear in time.", people[1], IncidentStatus.Open, north),
            I(9, 14, "Building 100 Room 105", IncidentCategory.Injury, IncidentSeverity.Low,
                "Minor cut to hand while opening a supply carton. First aid applied on site.", people[2], IncidentStatus.UnderReview, north),
            I(30, 8, "Building 300 Paint Shop", IncidentCategory.Fire, IncidentSeverity.High,
                "Small solvent fire in a waste bin, extinguished with a CO2 extinguisher.", people[0], IncidentStatus.Closed, south));

        await db.SaveChangesAsync(ct);
    }
}
