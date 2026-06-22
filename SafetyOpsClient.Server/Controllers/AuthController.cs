using Microsoft.AspNetCore.Mvc;

namespace SafetyOpsClient.Server.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string CookieName = "SafetyOps.Authentication";
        private const string ValidUsername = "admin";
        private const string ValidPassword = "admin";

        [HttpPost("/auth/login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == ValidUsername && request.Password == ValidPassword)
            {
                Response.Cookies.Append(CookieName, "authenticated", new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
                return Ok(new { success = true });
            }
            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

        [HttpGet("/auth/check")]
        public IActionResult Check()
        {
            return Ok(new { authenticated = Request.Cookies.ContainsKey(CookieName) });
        }

        [HttpPost("/auth/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(CookieName);
            return Ok(new { success = true });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
