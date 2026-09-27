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

        Person P(string first, string last, string middle, string gender, string dept, string category, string subscription, string number) =>
            new() { FirstName = first, LastName = last, MiddleName = middle, Gender = gender, Department = dept, EmployeeCategory = category, Subscription = subscription, EmployeeNumber = number };

        var people = new[]
        {
            P("John",   "Smith",    "A", "Male",   "Engineering",     "Full Time",  "Standard", "1000001"),
            P("Jane",   "Smith",    "B", "Female", "Operations",      "Full Time",  "Basic",    "1000002"),
            P("Robert", "Smith",    "C", "Male",   "Safety",          "Full Time",  "Premium",  "1000003"),
            P("Mary",   "Smith",    "D", "Female", "Finance",         "Contractor", "Standard", "1000004"),
            P("John",   "Doe",      "E", "Male",   "Finance",         "Full Time",  "Standard", "1000005"),
            P("John",   "Johnson",  "F", "Male",   "Human Resources", "Part Time",  "Basic",    "1000006"),
            P("John",   "Williams", "G", "Male",   "Engineering",     "Full Time",  "Premium",  "1000007"),
            P("Alice",  "Anderson", "H", "Female", "Operations",      "Full Time",  "Premium",  "1000008"),
            P("Carol",  "Davis",    "I", "Female", "Finance",         "Part Time",  "Standard", "1000009"),
            P("David",  "Wilson",   "J", "Male",   "Human Resources", "Full Time",  "Premium",  "1000010"),
        };
        db.People.AddRange(people);

        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        var courses = await db.Courses.ToDictionaryAsync(c => c.Code, ct);
        TrainingClass C(string code, int daysAgo, string location) =>
            new() { Course = courses[code], ClassDate = today.AddDays(-daysAgo), Location = location };

        db.TrainingClasses.AddRange(
            C("ELV-001", 10, "Building 100 Room 101"),
            C("ELV-001", 5, "Building 200 Room 202"),
            C("ELV-001", 20, "Building 300 Room 303"),
            C("ELH-001", 15, "Building 400 Room 404"),
            C("ELS-001", 25, "Building 100 Room 105"));

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

        await db.SaveChangesAsync(ct);
    }
}
