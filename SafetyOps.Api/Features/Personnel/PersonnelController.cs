using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Personnel;

/// <summary>Personnel records.</summary>
[Route("api/personnel")]
public class PersonnelController(IPersonnelService personnel) : ApiControllerBase
{
    /// <summary>Lists personnel, sorted by last then first name. Search matches first, middle, and last name and department.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<PersonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResult<PersonDto>> List([FromQuery] ListQuery query, CancellationToken ct) =>
        await personnel.ListAsync(query, ct);

    /// <summary>Gets one person.</summary>
    [HttpGet("{id:int}", Name = "GetPerson")]
    [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDto>> Get(int id, CancellationToken ct) =>
        await personnel.GetAsync(id, ct) is { } person ? person : Failure(ServiceError.NotFound("Person not found."));

    /// <summary>Creates a person.</summary>
    [HttpPost]
    [ProducesResponseType<PersonDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonDto>> Create(PersonRequest request, CancellationToken ct)
    {
        var result = await personnel.CreateAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : CreatedAtRoute("GetPerson", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Replaces a person's details.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDto>> Update(int id, PersonRequest request, CancellationToken ct) =>
        OkOrFailure(await personnel.UpdateAsync(id, request, ct));

    /// <summary>Deletes a person. Fails with 409 if the person has medical surveillance appointments.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct) =>
        NoContentOrFailure(await personnel.DeleteAsync(id, ct));
}
