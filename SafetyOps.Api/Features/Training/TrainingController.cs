using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Access;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Training;

/// <summary>Training classes and the course catalog.</summary>
[Route("api/training")]
[Authorize(Policy = AccessPolicies.Read)]
public class TrainingController(ITrainingService training) : ApiControllerBase
{
    /// <summary>Lists classes, newest first. Search matches course title and location.</summary>
    [HttpGet("classes")]
    [ProducesResponseType<PagedResult<TrainingClassDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResult<TrainingClassDto>> ListClasses([FromQuery] ListQuery query, CancellationToken ct) =>
        await training.ListClassesAsync(query, ct);

    /// <summary>Gets one class.</summary>
    [HttpGet("classes/{id:int}", Name = "GetTrainingClass")]
    [ProducesResponseType<TrainingClassDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrainingClassDto>> GetClass(int id, CancellationToken ct) =>
        await training.GetClassAsync(id, ct) is { } cls ? cls : Failure(TrainingService.ClassNotFound);

    /// <summary>Creates a class.</summary>
    [HttpPost("classes")]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<TrainingClassDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrainingClassDto>> CreateClass(TrainingClassRequest request, CancellationToken ct)
    {
        var result = await training.CreateClassAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : CreatedAtRoute("GetTrainingClass", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Replaces a class's details.</summary>
    [HttpPut("classes/{id:int}")]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<TrainingClassDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrainingClassDto>> UpdateClass(int id, TrainingClassRequest request, CancellationToken ct) =>
        OkOrFailure(await training.UpdateClassAsync(id, request, ct));

    /// <summary>Deletes a class.</summary>
    [HttpDelete("classes/{id:int}")]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteClass(int id, CancellationToken ct) =>
        NoContentOrFailure(await training.DeleteClassAsync(id, ct));

    /// <summary>Lists the course catalog. Search matches the title.</summary>
    [HttpGet("courses")]
    [ProducesResponseType<IReadOnlyList<CourseDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CourseDto>> GetCourses([FromQuery] string? search, CancellationToken ct) =>
        await training.GetCoursesAsync(search, ct);
}
