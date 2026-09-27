using Microsoft.AspNetCore.Mvc;

namespace SafetyOps.Api.Features.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Maps a service <see cref="Error"/> to its HTTP status with a <c>{ message }</c> body.</summary>
    protected IActionResult Failure(Error error) => error.Kind switch
    {
        ErrorKind.NotFound => NotFound(new { message = error.Message }),
        ErrorKind.Conflict => Conflict(new { message = error.Message }),
        _ => BadRequest(new { message = error.Message }),
    };
}
