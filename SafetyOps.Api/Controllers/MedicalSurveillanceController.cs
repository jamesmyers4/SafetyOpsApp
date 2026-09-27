using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Controllers
{
    public class AppointmentRecord
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public int PersonId { get; set; }
        public List<StressorRecord> Stressors { get; set; } = [];
    }

    public class StressorRecord
    {
        public string StressorId { get; set; } = string.Empty;
        public string StressorName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
    }

    public class PersonOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WorkTaskRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<StressorRecord> Stressors { get; set; } = [];
        public List<string> ExamTypeOptions { get; set; } = [];
    }

    [ApiController]
    public class MedicalSurveillanceController(AppDbContext db) : ControllerBase
    {
        [HttpPost("/api/medical-surveillance/appointments")]
        public async Task<IActionResult> Create([FromBody] AppointmentRecord record)
        {
            var appointment = new MedicalAppointment();
            if (await ApplyAsync(record, appointment) is { } error)
                return BadRequest(new { message = error });
            db.MedicalAppointments.Add(appointment);
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "Appointment created", id = appointment.Id });
        }

        [HttpGet("/api/medical-surveillance/appointments")]
        public async Task<IActionResult> GetAppointments([FromQuery] string? search = null)
        {
            var query = Appointments();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var lower = term.ToLowerInvariant();
                // Kept from the original API: the words "appointment(s)" match everything. Removed in Session 6.
                if (lower is not ("appointment" or "appointments"))
                {
                    var pattern = SqlLike.Contains(term);
                    var hasDate = DateFormats.TryParse(term, out var date);
                    var hasId = int.TryParse(term, out var id);
                    query = query.Where(a =>
                        EF.Functions.Like(a.Person.FirstName + " " + a.Person.LastName, pattern, SqlLike.Escape) ||
                        (hasDate && a.Date == date) ||
                        (hasId && a.Id == id));
                }
            }
            var appointments = await query.OrderByDescending(a => a.Id).ToListAsync();
            return Ok(appointments.Select(ToRecord));
        }

        [HttpGet("/api/medical-surveillance/appointments/{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await Appointments().FirstOrDefaultAsync(a => a.Id == id);
            return appointment is null
                ? NotFound(new { message = "Appointment not found" })
                : Ok(ToRecord(appointment));
        }

        [HttpPut("/api/medical-surveillance/appointments/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentRecord record)
        {
            var appointment = await db.MedicalAppointments.Include(a => a.Stressors).FirstOrDefaultAsync(a => a.Id == id);
            if (appointment is null)
                return NotFound(new { message = "Appointment not found" });
            if (await ApplyAsync(record, appointment) is { } error)
                return BadRequest(new { message = error });
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "Appointment updated" });
        }

        [HttpDelete("/api/medical-surveillance/appointments/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await db.MedicalAppointments.Include(a => a.Stressors).FirstOrDefaultAsync(a => a.Id == id);
            if (appointment is null)
                return NotFound(new { message = "Appointment not found" });
            db.MedicalAppointments.Remove(appointment);
            await db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("/api/medical-surveillance/persons")]
        public async Task<IActionResult> GetPersons([FromQuery] string? search = null)
        {
            var query = db.People.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => EF.Functions.Like(p.FirstName + " " + p.LastName, SqlLike.Contains(search), SqlLike.Escape));
            var persons = await query
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Select(p => new PersonOption { Id = p.Id, Name = p.FirstName + " " + p.LastName })
                .ToListAsync();
            return Ok(persons);
        }

        [HttpGet("/api/medical-surveillance/work-tasks")]
        public async Task<IActionResult> GetWorkTasks()
        {
            var tasks = await db.WorkTasks.AsNoTracking().Include(t => t.Stressors).OrderBy(t => t.Id).ToListAsync();
            return Ok(tasks.Select(t => new WorkTaskRecord
            {
                Id = t.Code,
                Name = t.Name,
                Stressors = t.Stressors.Select(s => new StressorRecord { StressorId = s.Code, StressorName = s.Name }).ToList(),
                ExamTypeOptions = t.ExamTypeOptions,
            }));
        }

        private IQueryable<MedicalAppointment> Appointments() =>
            db.MedicalAppointments.AsNoTracking()
                .Include(a => a.Person)
                .Include(a => a.Stressors).ThenInclude(s => s.Stressor)
                .AsSplitQuery();

        private static AppointmentRecord ToRecord(MedicalAppointment a) => new()
        {
            Id = a.Id,
            Date = DateFormats.ToWire(a.Date),
            PersonId = a.PersonId,
            PersonName = a.Person.FullName,
            Stressors = a.Stressors
                .Select(s => new StressorRecord { StressorId = s.Stressor.Code, StressorName = s.Stressor.Name, ExamType = s.ExamType })
                .ToList(),
        };

        // Person and stressors are resolved by id/code; names in the request are ignored in favor of stored ones.
        private async Task<string?> ApplyAsync(AppointmentRecord record, MedicalAppointment appointment)
        {
            if (!DateFormats.TryParse(record.Date, out var date))
                return $"Invalid appointment date '{record.Date}'";
            if (!await db.People.AnyAsync(p => p.Id == record.PersonId))
                return $"Unknown person {record.PersonId}";

            var codes = record.Stressors.Select(s => s.StressorId).Distinct().ToList();
            var stressors = await db.Stressors.Where(s => codes.Contains(s.Code)).ToDictionaryAsync(s => s.Code);
            if (codes.FirstOrDefault(c => !stressors.ContainsKey(c)) is { } unknown)
                return $"Unknown stressor '{unknown}'";

            appointment.Date = date;
            appointment.PersonId = record.PersonId;
            appointment.Stressors.Clear();
            foreach (var s in record.Stressors.DistinctBy(s => s.StressorId))
                appointment.Stressors.Add(new AppointmentStressor { StressorId = stressors[s.StressorId].Id, ExamType = s.ExamType });
            return null;
        }
    }
}
