using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Personnel;

public class PersonnelController(IPersonnelService personnel) : ApiControllerBase
{
    [HttpGet("/api/personnel/users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, CancellationToken ct) =>
        Ok(await personnel.SearchAsync(search, ct));

    [HttpPost("/api/personnel/create")]
    public async Task<IActionResult> Create([FromBody] PersonRequest request, CancellationToken ct)
    {
        var result = await personnel.CreateAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "User created successfully", id = result.Value!.Id });
    }

    [HttpGet("/api/personnel/users/{id}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken ct) =>
        await personnel.GetAsync(id, ct) is { } person
            ? Ok(person)
            : NotFound(new { message = "User not found" });

    [HttpPut("/api/personnel/users/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonRequest request, CancellationToken ct)
    {
        var result = await personnel.UpdateAsync(id, request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "User updated successfully" });
    }

    [HttpDelete("/api/personnel/users/{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await personnel.DeleteAsync(id, ct);
        return result.Error is { } error ? Failure(error) : Ok(new { success = true });
    }
}
