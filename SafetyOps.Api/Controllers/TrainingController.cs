using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Controllers
{
    public class TrainingClassRecord
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
    public class TrainingController(AppDbContext db) : ControllerBase
    {
        [HttpPost("/api/training/classes")]
        public async Task<IActionResult> Create([FromBody] TrainingClassRecord record)
        {
            var cls = new TrainingClass();
            if (await ApplyAsync(record, cls) is { } error)
                return BadRequest(new { message = error });
            db.TrainingClasses.Add(cls);
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "Class created", id = cls.Id });
        }

        [HttpGet("/api/training/classes")]
        public async Task<IActionResult> GetClasses([FromQuery] string? search = null)
        {
            var query = db.TrainingClasses.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = SqlLike.Contains(search);
                query = query.Where(c =>
                    EF.Functions.Like(c.Course.Title, pattern, SqlLike.Escape) ||
                    EF.Functions.Like(c.Location, pattern, SqlLike.Escape));
            }
            var rows = await query
                .OrderByDescending(c => c.Id)
                .Select(c => new { c.Id, c.Course.Title, c.Course.Code, c.ClassDate, c.Location })
                .ToListAsync();
            return Ok(rows.Select(c => new TrainingClassRecord
            {
                Id = c.Id, CourseTitle = c.Title, CourseId = c.Code, ClassDate = DateFormats.ToWire(c.ClassDate), Location = c.Location,
            }));
        }

        [HttpGet("/api/training/classes/{id}")]
        public async Task<IActionResult> GetClass(int id)
        {
            var cls = await db.TrainingClasses.AsNoTracking().Include(c => c.Course).FirstOrDefaultAsync(c => c.Id == id);
            return cls is null
                ? NotFound(new { message = "Class not found" })
                : Ok(new TrainingClassRecord
                {
                    Id = cls.Id, CourseTitle = cls.Course.Title, CourseId = cls.Course.Code, ClassDate = DateFormats.ToWire(cls.ClassDate), Location = cls.Location,
                });
        }

        [HttpPut("/api/training/classes/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TrainingClassRecord record)
        {
            var cls = await db.TrainingClasses.FindAsync(id);
            if (cls is null)
                return NotFound(new { message = "Class not found" });
            if (await ApplyAsync(record, cls) is { } error)
                return BadRequest(new { message = error });
            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "Class updated" });
        }

        [HttpDelete("/api/training/classes/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await db.TrainingClasses.Where(c => c.Id == id).ExecuteDeleteAsync();
            return deleted > 0
                ? Ok(new { success = true })
                : NotFound(new { message = "Class not found" });
        }

        [HttpGet("/api/training/courses")]
        public async Task<IActionResult> GetCourses([FromQuery] string? search = null)
        {
            var query = db.Courses.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => EF.Functions.Like(c.Title, SqlLike.Contains(search), SqlLike.Escape));
            var courses = await query
                .OrderBy(c => c.Id)
                .Select(c => new CourseRecord { Id = c.Code, Title = c.Title })
                .ToListAsync();
            return Ok(courses);
        }

        // The course is identified by its code; the title in the request is ignored in favor of the catalog's.
        private async Task<string?> ApplyAsync(TrainingClassRecord record, TrainingClass cls)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Code == record.CourseId);
            if (course is null)
                return $"Unknown course '{record.CourseId}'";
            if (!DateFormats.TryParse(record.ClassDate, out var date))
                return $"Invalid class date '{record.ClassDate}'";
            cls.CourseId = course.Id;
            cls.ClassDate = date;
            cls.Location = record.Location;
            return null;
        }
    }
}
