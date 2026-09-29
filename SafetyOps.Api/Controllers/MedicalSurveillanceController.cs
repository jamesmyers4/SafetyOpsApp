using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace SafetyOps.Api.Controllers
{
    public class MedicalAppointment
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public int PersonId { get; set; }
        public List<Stressor> Stressors { get; set; } = [];
    }

    public class Stressor
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

    public class WorkTask
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<Stressor> Stressors { get; set; } = [];
        public List<string> ExamTypeOptions { get; set; } = [];
    }

    [ApiController]
    public class MedicalSurveillanceController : ControllerBase
    {
        private static int _nextId = 1;
        private static readonly ConcurrentDictionary<int, MedicalAppointment> _appointments = new();

        private static readonly List<PersonOption> _persons =
        [
            new PersonOption { Id = 1, Name = "John Smith" },
            new PersonOption { Id = 2, Name = "Jane Doe" },
            new PersonOption { Id = 3, Name = "Robert Johnson" },
            new PersonOption { Id = 4, Name = "Mary Williams" },
            new PersonOption { Id = 5, Name = "James Brown" },
        ];

        private static readonly List<WorkTask> _workTasks =
        [
            new WorkTask
            {
                Id = "WT-001", Name = "Chemical Exposure - Solvents",
                Stressors = [ new Stressor { StressorId = "STR-001", StressorName = "Solvent Exposure" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit", "Return to Duty" ],
            },
            new WorkTask
            {
                Id = "WT-002", Name = "Noise Hazard - Industrial",
                Stressors = [ new Stressor { StressorId = "STR-002", StressorName = "Noise Exposure" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit" ],
            },
            new WorkTask
            {
                Id = "WT-003", Name = "Respiratory Hazard - Dust",
                Stressors = [ new Stressor { StressorId = "STR-003", StressorName = "Dust Inhalation" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit", "Special" ],
            },
        ];

        static MedicalSurveillanceController()
        {
            var today = DateTime.Today;
            var seed = new[]
            {
                new MedicalAppointment
                {
                    Date = today.AddDays(-3).ToString("MM/dd/yyyy"), PersonName = "John Smith", PersonId = 1,
                    Stressors = [ new Stressor { StressorId = "STR-001", StressorName = "Solvent Exposure", ExamType = "Initial" } ],
                },
                new MedicalAppointment
                {
                    Date = today.AddDays(-7).ToString("MM/dd/yyyy"), PersonName = "Jane Doe", PersonId = 2,
                    Stressors = [ new Stressor { StressorId = "STR-002", StressorName = "Noise Exposure", ExamType = "Periodic" } ],
                },
                new MedicalAppointment
                {
                    Date = today.AddDays(-14).ToString("MM/dd/yyyy"), PersonName = "Robert Johnson", PersonId = 3,
                    Stressors = [ new Stressor { StressorId = "STR-003", StressorName = "Dust Inhalation", ExamType = "Exit" } ],
                },
            };
            foreach (var a in seed)
            {
                a.Id = _nextId++;
                _appointments[a.Id] = a;
            }
        }

        [HttpPost("/api/medical-surveillance/appointments")]
        public IActionResult Create([FromBody] MedicalAppointment appointment)
        {
            appointment.Id = _nextId++;
            _appointments[appointment.Id] = appointment;
            return Ok(new { success = true, message = "Appointment created", id = appointment.Id });
        }

        [HttpGet("/api/medical-surveillance/appointments")]
        public IActionResult GetAppointments([FromQuery] string? search = null)
        {
            var appts = _appointments.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLowerInvariant();
                appts = appts.Where(a =>
                    a.PersonName.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                    a.Date.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                    a.Id.ToString().Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                    lower == "appointment" || lower == "appointments");
            }
            return Ok(appts.OrderByDescending(a => a.Id).ToList());
        }

        [HttpGet("/api/medical-surveillance/appointments/{id}")]
        public IActionResult GetAppointment(int id)
        {
            return _appointments.TryGetValue(id, out var appt)
                ? Ok(appt)
                : NotFound(new { message = "Appointment not found" });
        }

        [HttpPut("/api/medical-surveillance/appointments/{id}")]
        public IActionResult Update(int id, [FromBody] MedicalAppointment appointment)
        {
            if (!_appointments.ContainsKey(id))
                return NotFound(new { message = "Appointment not found" });
            appointment.Id = id;
            _appointments[id] = appointment;
            return Ok(new { success = true, message = "Appointment updated" });
        }

        [HttpDelete("/api/medical-surveillance/appointments/{id}")]
        public IActionResult Delete(int id)
        {
            return _appointments.TryRemove(id, out _)
                ? Ok(new { success = true })
                : NotFound(new { message = "Appointment not found" });
        }

        [HttpGet("/api/medical-surveillance/persons")]
        public IActionResult GetPersons([FromQuery] string? search = null)
        {
            var persons = _persons.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                persons = persons.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            return Ok(persons.ToList());
        }

        [HttpGet("/api/medical-surveillance/work-tasks")]
        public IActionResult GetWorkTasks()
        {
            return Ok(_workTasks);
        }
    }
}
