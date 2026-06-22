using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace SafetyOpsClient.Server.Controllers
{
    public class OmssAppointment
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public int PersonId { get; set; }
        public List<OmssStressor> Stressors { get; set; } = [];
    }

    public class OmssStressor
    {
        public string StressorId { get; set; } = string.Empty;
        public string StressorName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
    }

    public class OmssPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class OmssWorkTask
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<OmssStressor> Stressors { get; set; } = [];
        public List<string> ExamTypeOptions { get; set; } = [];
    }

    [ApiController]
    public class OMSSController : ControllerBase
    {
        private static int _nextId = 1;
        private static readonly ConcurrentDictionary<int, OmssAppointment> _appointments = new();

        private static readonly List<OmssPerson> _persons =
        [
            new OmssPerson { Id = 1, Name = "John Smith" },
            new OmssPerson { Id = 2, Name = "Jane Doe" },
            new OmssPerson { Id = 3, Name = "Robert Johnson" },
            new OmssPerson { Id = 4, Name = "Mary Williams" },
            new OmssPerson { Id = 5, Name = "James Brown" },
        ];

        private static readonly List<OmssWorkTask> _workTasks =
        [
            new OmssWorkTask
            {
                Id = "WT-001", Name = "Chemical Exposure - Solvents",
                Stressors = [ new OmssStressor { StressorId = "STR-001", StressorName = "Solvent Exposure" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit", "Return to Duty" ],
            },
            new OmssWorkTask
            {
                Id = "WT-002", Name = "Noise Hazard - Industrial",
                Stressors = [ new OmssStressor { StressorId = "STR-002", StressorName = "Noise Exposure" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit" ],
            },
            new OmssWorkTask
            {
                Id = "WT-003", Name = "Respiratory Hazard - Dust",
                Stressors = [ new OmssStressor { StressorId = "STR-003", StressorName = "Dust Inhalation" } ],
                ExamTypeOptions = [ "Initial", "Periodic", "Exit", "Special" ],
            },
        ];

        static OMSSController()
        {
            var today = DateTime.Today;
            var seed = new[]
            {
                new OmssAppointment
                {
                    Date = today.AddDays(-3).ToString("MM/dd/yyyy"), PersonName = "John Smith", PersonId = 1,
                    Stressors = [ new OmssStressor { StressorId = "STR-001", StressorName = "Solvent Exposure", ExamType = "Initial" } ],
                },
                new OmssAppointment
                {
                    Date = today.AddDays(-7).ToString("MM/dd/yyyy"), PersonName = "Jane Doe", PersonId = 2,
                    Stressors = [ new OmssStressor { StressorId = "STR-002", StressorName = "Noise Exposure", ExamType = "Periodic" } ],
                },
                new OmssAppointment
                {
                    Date = today.AddDays(-14).ToString("MM/dd/yyyy"), PersonName = "Robert Johnson", PersonId = 3,
                    Stressors = [ new OmssStressor { StressorId = "STR-003", StressorName = "Dust Inhalation", ExamType = "Exit" } ],
                },
            };
            foreach (var a in seed)
            {
                a.Id = _nextId++;
                _appointments[a.Id] = a;
            }
        }

        [HttpPost("/n/safetyops/omss/appointments")]
        public IActionResult Create([FromBody] OmssAppointment appointment)
        {
            appointment.Id = _nextId++;
            _appointments[appointment.Id] = appointment;
            return Ok(new { success = true, message = "Appointment created", id = appointment.Id });
        }

        [HttpGet("/n/safetyops/omss/appointments")]
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

        [HttpGet("/n/safetyops/omss/appointments/{id}")]
        public IActionResult GetAppointment(int id)
        {
            return _appointments.TryGetValue(id, out var appt)
                ? Ok(appt)
                : NotFound(new { message = "Appointment not found" });
        }

        [HttpPut("/n/safetyops/omss/appointments/{id}")]
        public IActionResult Update(int id, [FromBody] OmssAppointment appointment)
        {
            if (!_appointments.ContainsKey(id))
                return NotFound(new { message = "Appointment not found" });
            appointment.Id = id;
            _appointments[id] = appointment;
            return Ok(new { success = true, message = "Appointment updated" });
        }

        [HttpDelete("/n/safetyops/omss/appointments/{id}")]
        public IActionResult Delete(int id)
        {
            return _appointments.TryRemove(id, out _)
                ? Ok(new { success = true })
                : NotFound(new { message = "Appointment not found" });
        }

        [HttpGet("/n/safetyops/omss/persons")]
        public IActionResult GetPersons([FromQuery] string? search = null)
        {
            var persons = _persons.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                persons = persons.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            return Ok(persons.ToList());
        }

        [HttpGet("/n/safetyops/omss/worktasks")]
        public IActionResult GetWorkTasks()
        {
            return Ok(_workTasks);
        }
    }
}
