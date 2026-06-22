using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace SafetyOpsClient.Server.Controllers
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
    public class PersonnelController : ControllerBase
    {
        private static int _nextId = 1;
        private static readonly ConcurrentDictionary<int, UserRecord> _users = new();

        static PersonnelController()
        {
            var seed = new[]
            {
                new UserRecord { FirstName = "John",    LastName = "Smith",    MiddleName = "A", Gender = "Male",   Department = "Engineering",     EmployeeCategory = "Full Time",  Subscription = "Standard", EmployeeNumber = "1000001" },
                new UserRecord { FirstName = "Jane",    LastName = "Smith",    MiddleName = "B", Gender = "Female", Department = "Operations",      EmployeeCategory = "Full Time",  Subscription = "Basic",    EmployeeNumber = "1000002" },
                new UserRecord { FirstName = "Robert",  LastName = "Smith",    MiddleName = "C", Gender = "Male",   Department = "Safety",          EmployeeCategory = "Full Time",  Subscription = "Premium",  EmployeeNumber = "1000003" },
                new UserRecord { FirstName = "Mary",    LastName = "Smith",    MiddleName = "D", Gender = "Female", Department = "Finance",         EmployeeCategory = "Contractor", Subscription = "Standard", EmployeeNumber = "1000004" },
                new UserRecord { FirstName = "John",    LastName = "Doe",      MiddleName = "E", Gender = "Male",   Department = "Finance",         EmployeeCategory = "Full Time",  Subscription = "Standard", EmployeeNumber = "1000005" },
                new UserRecord { FirstName = "John",    LastName = "Johnson",  MiddleName = "F", Gender = "Male",   Department = "Human Resources", EmployeeCategory = "Part Time",  Subscription = "Basic",    EmployeeNumber = "1000006" },
                new UserRecord { FirstName = "John",    LastName = "Williams", MiddleName = "G", Gender = "Male",   Department = "Engineering",     EmployeeCategory = "Full Time",  Subscription = "Premium",  EmployeeNumber = "1000007" },
                new UserRecord { FirstName = "Alice",   LastName = "Anderson", MiddleName = "H", Gender = "Female", Department = "Operations",      EmployeeCategory = "Full Time",  Subscription = "Premium",  EmployeeNumber = "1000008" },
                new UserRecord { FirstName = "Carol",   LastName = "Davis",    MiddleName = "I", Gender = "Female", Department = "Finance",         EmployeeCategory = "Part Time",  Subscription = "Standard", EmployeeNumber = "1000009" },
                new UserRecord { FirstName = "David",   LastName = "Wilson",   MiddleName = "J", Gender = "Male",   Department = "Human Resources", EmployeeCategory = "Full Time",  Subscription = "Premium",  EmployeeNumber = "1000010" },
            };
            foreach (var u in seed)
            {
                u.Id = _nextId++;
                _users[u.Id] = u;
            }
        }

        [HttpGet("/n/safetyops/personnel/users")]
        public IActionResult GetUsers([FromQuery] string? search = null)
        {
            var users = _users.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.MiddleName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Department.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            return Ok(users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList());
        }

        [HttpPost("/n/safetyops/personnel/create")]
        public IActionResult Create([FromBody] UserRecord user)
        {
            user.Id = _nextId++;
            _users[user.Id] = user;
            return Ok(new { success = true, message = "User created successfully", id = user.Id });
        }

        [HttpGet("/n/safetyops/personnel/users/{id}")]
        public IActionResult GetUser(int id)
        {
            return _users.TryGetValue(id, out var user)
                ? Ok(user)
                : NotFound(new { message = "User not found" });
        }

        [HttpPut("/n/safetyops/personnel/users/{id}")]
        public IActionResult Update(int id, [FromBody] UserRecord user)
        {
            if (!_users.ContainsKey(id))
                return NotFound(new { message = "User not found" });
            user.Id = id;
            _users[id] = user;
            return Ok(new { success = true, message = "User updated successfully" });
        }

        [HttpDelete("/n/safetyops/personnel/users/{id}")]
        public IActionResult Delete(int id)
        {
            return _users.TryRemove(id, out _)
                ? Ok(new { success = true })
                : NotFound(new { message = "User not found" });
        }
    }
}
