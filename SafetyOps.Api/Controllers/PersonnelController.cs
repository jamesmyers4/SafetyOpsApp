using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Controllers
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmployeeCategory { get; set; } = string.Empty;
        public string Subscription { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
    }

    [ApiController]
    public class PersonnelController(AppDbContext db) : ControllerBase
    {
        [HttpGet("/api/personnel/users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search = null)
        {
            var query = db.People.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = SqlLike.Contains(search);
                query = query.Where(p =>
                    EF.Functions.Like(p.FirstName, pattern, SqlLike.Escape) ||
                    EF.Functions.Like(p.LastName, pattern, SqlLike.Escape) ||
                    EF.Functions.Like(p.MiddleName, pattern, SqlLike.Escape) ||
                    EF.Functions.Like(p.Department, pattern, SqlLike.Escape));
            }
            var users = await query
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Select(p => ToRecord(p))
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost("/api/personnel/create")]
        public async Task<IActionResult> Create([FromBody] UserRecord user)
        {
            var person = new Person();
            Apply(user, person);
            db.People.Add(person);
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "User created successfully", id = person.Id });
        }

        [HttpGet("/api/personnel/users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var person = await db.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return person is null
                ? NotFound(new { message = "User not found" })
                : Ok(ToRecord(person));
        }

        [HttpPut("/api/personnel/users/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserRecord user)
        {
            var person = await db.People.FindAsync(id);
            if (person is null)
                return NotFound(new { message = "User not found" });
            Apply(user, person);
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "User updated successfully" });
        }

        [HttpDelete("/api/personnel/users/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var person = await db.People.FindAsync(id);
            if (person is null)
                return NotFound(new { message = "User not found" });
            if (await db.MedicalAppointments.AnyAsync(a => a.PersonId == id))
                return Conflict(new { message = "User has medical surveillance appointments and cannot be deleted" });
            db.People.Remove(person);
            await db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        private static UserRecord ToRecord(Person p) => new()
        {
            Id = p.Id, FirstName = p.FirstName, LastName = p.LastName, MiddleName = p.MiddleName, Gender = p.Gender,
            Department = p.Department, EmployeeCategory = p.EmployeeCategory, Subscription = p.Subscription, EmployeeNumber = p.EmployeeNumber,
        };

        private static void Apply(UserRecord r, Person p)
        {
            p.FirstName = r.FirstName; p.LastName = r.LastName; p.MiddleName = r.MiddleName; p.Gender = r.Gender;
            p.Department = r.Department; p.EmployeeCategory = r.EmployeeCategory; p.Subscription = r.Subscription; p.EmployeeNumber = r.EmployeeNumber;
        }
    }
}
