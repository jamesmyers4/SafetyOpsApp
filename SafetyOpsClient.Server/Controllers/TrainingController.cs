using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace SafetyOpsClient.Server.Controllers
{
    public class TrainingClass
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string ClassDate { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class CourseRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    [ApiController]
    public class TrainingController : ControllerBase
    {
        private static int _nextId = 1;
        private static readonly ConcurrentDictionary<int, TrainingClass> _classes = new();

        private static readonly List<CourseRecord> _courses =
        [
            new CourseRecord { Id = "ELV-001", Title = "Electrical - Low Voltage" },
            new CourseRecord { Id = "ELH-001", Title = "Electrical - High Voltage" },
            new CourseRecord { Id = "ELS-001", Title = "Electrical - Safety Basics" },
            new CourseRecord { Id = "FPS-001", Title = "Fire Prevention and Safety" },
            new CourseRecord { Id = "HAZ-001", Title = "Hazardous Materials Handling" },
            new CourseRecord { Id = "PPE-001", Title = "Personal Protective Equipment" },
            new CourseRecord { Id = "FAC-001", Title = "First Aid and CPR" },
            new CourseRecord { Id = "LOT-001", Title = "Lockout/Tagout Procedures" },
        ];

        static TrainingController()
        {
            var today = DateTime.Today;
            var seed = new[]
            {
                new TrainingClass { CourseTitle = "Electrical - Low Voltage",  CourseId = "ELV-001", ClassDate = today.AddDays(-10).ToString("MM/dd/yyyy"), Location = "Building 100 Room 101" },
                new TrainingClass { CourseTitle = "Electrical - Low Voltage",  CourseId = "ELV-001", ClassDate = today.AddDays(-5).ToString("MM/dd/yyyy"),  Location = "Building 200 Room 202" },
                new TrainingClass { CourseTitle = "Electrical - Low Voltage",  CourseId = "ELV-001", ClassDate = today.AddDays(-20).ToString("MM/dd/yyyy"), Location = "Building 300 Room 303" },
                new TrainingClass { CourseTitle = "Electrical - High Voltage", CourseId = "ELH-001", ClassDate = today.AddDays(-15).ToString("MM/dd/yyyy"), Location = "Building 400 Room 404" },
                new TrainingClass { CourseTitle = "Electrical - Safety Basics",CourseId = "ELS-001", ClassDate = today.AddDays(-25).ToString("MM/dd/yyyy"), Location = "Building 100 Room 105" },
            };
            foreach (var c in seed)
            {
                c.Id = _nextId++;
                _classes[c.Id] = c;
            }
        }

        [HttpPost("/n/safetyops/training/classes")]
        public IActionResult Create([FromBody] TrainingClass cls)
        {
            cls.Id = _nextId++;
            _classes[cls.Id] = cls;
            return Ok(new { success = true, message = "Class created", id = cls.Id });
        }

        [HttpGet("/n/safetyops/training/classes")]
        public IActionResult GetClasses([FromQuery] string? search = null)
        {
            var classes = _classes.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                classes = classes.Where(c =>
                    c.CourseTitle.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
            return Ok(classes.OrderByDescending(c => c.Id).ToList());
        }

        [HttpGet("/n/safetyops/training/classes/{id}")]
        public IActionResult GetClass(int id)
        {
            return _classes.TryGetValue(id, out var cls)
                ? Ok(cls)
                : NotFound(new { message = "Class not found" });
        }

        [HttpPut("/n/safetyops/training/classes/{id}")]
        public IActionResult Update(int id, [FromBody] TrainingClass cls)
        {
            if (!_classes.ContainsKey(id))
                return NotFound(new { message = "Class not found" });
            cls.Id = id;
            _classes[id] = cls;
            return Ok(new { success = true, message = "Class updated" });
        }

        [HttpDelete("/n/safetyops/training/classes/{id}")]
        public IActionResult Delete(int id)
        {
            return _classes.TryRemove(id, out _)
                ? Ok(new { success = true })
                : NotFound(new { message = "Class not found" });
        }

        [HttpGet("/n/safetyops/training/courses")]
        public IActionResult GetCourses([FromQuery] string? search = null)
        {
            var courses = _courses.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                courses = courses.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
            return Ok(courses.ToList());
        }
    }
}
