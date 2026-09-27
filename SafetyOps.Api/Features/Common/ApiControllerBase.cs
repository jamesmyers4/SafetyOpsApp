using Microsoft.AspNetCore.Mvc;

namespace SafetyOps.Api.Features.Common;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Maps a service <see cref="Error"/> to RFC 9457 problem details: 404 and 409 as
    /// <see cref="ProblemDetails"/>, invalid input as <see cref="ValidationProblemDetails"/>.
    /// </summary>
    protected ActionResult Failure(Error error)
    {
        switch (error.Kind)
        {
            case ErrorKind.NotFound:
                return Problem(detail: error.Message, statusCode: StatusCodes.Status404NotFound);
            case ErrorKind.Conflict:
                return Problem(detail: error.Message, statusCode: StatusCodes.Status409Conflict);
            default:
                ModelState.AddModelError(error.Field ?? string.Empty, error.Message);
                return ValidationProblem(ModelState);
        }
    }

    /// <summary>Returns the value on success, or the mapped failure.</summary>
    protected ActionResult<T> OkOrFailure<T>(Result<T> result) =>
        result.Error is { } error ? Failure(error) : Ok(result.Value);

    /// <summary>Returns 204 on success, or the mapped failure.</summary>
    protected ActionResult NoContentOrFailure(Result result) =>
        result.Error is { } error ? Failure(error) : NoContent();
}
