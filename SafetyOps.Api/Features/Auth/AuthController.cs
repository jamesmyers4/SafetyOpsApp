using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Auth;

/// <summary>Cookie sign-in for the SPA.</summary>
[Route("api/auth")]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    /// <summary>Signs in and sets the HttpOnly auth cookie.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await auth.ValidateCredentialsAsync(request.Username, request.Password, ct);
        if (user is null)
            return Problem(detail: "Invalid username or password.", statusCode: StatusCodes.Status401Unauthorized);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AuthService.CreatePrincipal(user));
        return new CurrentUserDto(user.Id, user.UserName, user.DisplayName);
    }

    /// <summary>Signs out and clears the auth cookie.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>Returns the signed-in user, or 401.</summary>
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        if (AuthService.GetUserId(User) is { } id && await auth.GetUserAsync(id, ct) is { } user)
            return user;

        // The cookie is valid but the account is gone.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Problem(statusCode: StatusCodes.Status401Unauthorized);
    }
}
