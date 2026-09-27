using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Access;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Incidents;

/// <summary>Workplace incident reports.</summary>
[Route("api/incidents")]
[Authorize(Policy = AccessPolicies.Read)]
public class IncidentsController(IIncidentService incidents) : ApiControllerBase
{
    /// <summary>Lists incidents, most recent first. Search matches location, description, and reporter; status and category filter exactly.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<IncidentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResult<IncidentDto>> List([FromQuery] IncidentListQuery query, CancellationToken ct) =>
        await incidents.ListAsync(query, ct);

    /// <summary>Gets one incident.</summary>
    [HttpGet("{id:int}", Name = "GetIncident")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> Get(int id, CancellationToken ct) =>
        await incidents.GetAsync(id, ct) is { } incident ? incident : Failure(IncidentService.NotFound);

    /// <summary>Reports an incident. Status defaults to Open.</summary>
    [HttpPost]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> Create(IncidentRequest request, CancellationToken ct)
    {
        var result = await incidents.CreateAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : CreatedAtRoute("GetIncident", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Replaces an incident's details. Omitting status keeps the current one.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> Update(int id, IncidentRequest request, CancellationToken ct) =>
        OkOrFailure(await incidents.UpdateAsync(id, request, ct));

    /// <summary>Deletes an incident.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AccessPolicies.Write)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct) =>
        NoContentOrFailure(await incidents.DeleteAsync(id, ct));
}
