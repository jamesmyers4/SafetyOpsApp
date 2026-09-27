using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Training;

public class TrainingController(ITrainingService training) : ApiControllerBase
{
    [HttpPost("/api/training/classes")]
    public async Task<IActionResult> Create([FromBody] TrainingClassRequest request, CancellationToken ct)
    {
        var result = await training.CreateClassAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "Class created", id = result.Value!.Id });
    }

    [HttpGet("/api/training/classes")]
    public async Task<IActionResult> GetClasses([FromQuery] string? search, CancellationToken ct) =>
        Ok(await training.SearchClassesAsync(search, ct));

    [HttpGet("/api/training/classes/{id}")]
    public async Task<IActionResult> GetClass(int id, CancellationToken ct) =>
        await training.GetClassAsync(id, ct) is { } cls
            ? Ok(cls)
            : NotFound(new { message = "Class not found" });

    [HttpPut("/api/training/classes/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TrainingClassRequest request, CancellationToken ct)
    {
        var result = await training.UpdateClassAsync(id, request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "Class updated" });
    }

    [HttpDelete("/api/training/classes/{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await training.DeleteClassAsync(id, ct);
        return result.Error is { } error ? Failure(error) : Ok(new { success = true });
    }

    [HttpGet("/api/training/courses")]
    public async Task<IActionResult> GetCourses([FromQuery] string? search, CancellationToken ct) =>
        Ok(await training.GetCoursesAsync(search, ct));
}
