using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.MedicalSurveillance;

/// <summary>Medical surveillance appointments and their lookups.</summary>
[Route("api/medical-surveillance")]
public class MedicalSurveillanceController(IMedicalSurveillanceService medical) : ApiControllerBase
{
    /// <summary>Lists appointments, newest first. Search matches the person's name, an exact date, or an id.</summary>
    [HttpGet("appointments")]
    [ProducesResponseType<PagedResult<AppointmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResult<AppointmentDto>> ListAppointments([FromQuery] ListQuery query, CancellationToken ct) =>
        await medical.ListAppointmentsAsync(query, ct);

    /// <summary>Gets one appointment.</summary>
    [HttpGet("appointments/{id:int}", Name = "GetAppointment")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id, CancellationToken ct) =>
        await medical.GetAppointmentAsync(id, ct) is { } appointment ? appointment : Failure(MedicalSurveillanceService.AppointmentNotFound);

    /// <summary>Creates an appointment.</summary>
    [HttpPost("appointments")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment(AppointmentRequest request, CancellationToken ct)
    {
        var result = await medical.CreateAppointmentAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : CreatedAtRoute("GetAppointment", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Replaces an appointment's date, person, and stressors.</summary>
    [HttpPut("appointments/{id:int}")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> UpdateAppointment(int id, AppointmentRequest request, CancellationToken ct) =>
        OkOrFailure(await medical.UpdateAppointmentAsync(id, request, ct));

    /// <summary>Deletes an appointment.</summary>
    [HttpDelete("appointments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAppointment(int id, CancellationToken ct) =>
        NoContentOrFailure(await medical.DeleteAppointmentAsync(id, ct));

    /// <summary>People who can be selected for evaluation. Search matches the full name.</summary>
    [HttpGet("persons")]
    [ProducesResponseType<IReadOnlyList<PersonOptionDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<PersonOptionDto>> GetPersons([FromQuery] string? search, CancellationToken ct) =>
        await medical.GetPersonOptionsAsync(search, ct);

    /// <summary>Work tasks with their stressors and allowed exam types.</summary>
    [HttpGet("work-tasks")]
    [ProducesResponseType<IReadOnlyList<WorkTaskDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<WorkTaskDto>> GetWorkTasks(CancellationToken ct) =>
        await medical.GetWorkTasksAsync(ct);
}
