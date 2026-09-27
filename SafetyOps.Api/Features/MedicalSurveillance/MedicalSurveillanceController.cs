using Microsoft.AspNetCore.Mvc;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.MedicalSurveillance;

public class MedicalSurveillanceController(IMedicalSurveillanceService medical) : ApiControllerBase
{
    [HttpPost("/api/medical-surveillance/appointments")]
    public async Task<IActionResult> Create([FromBody] AppointmentRequest request, CancellationToken ct)
    {
        var result = await medical.CreateAppointmentAsync(request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "Appointment created", id = result.Value!.Id });
    }

    [HttpGet("/api/medical-surveillance/appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] string? search, CancellationToken ct) =>
        Ok(await medical.SearchAppointmentsAsync(search, ct));

    [HttpGet("/api/medical-surveillance/appointments/{id}")]
    public async Task<IActionResult> GetAppointment(int id, CancellationToken ct) =>
        await medical.GetAppointmentAsync(id, ct) is { } appointment
            ? Ok(appointment)
            : NotFound(new { message = "Appointment not found" });

    [HttpPut("/api/medical-surveillance/appointments/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AppointmentRequest request, CancellationToken ct)
    {
        var result = await medical.UpdateAppointmentAsync(id, request, ct);
        return result.Error is { } error
            ? Failure(error)
            : Ok(new { success = true, message = "Appointment updated" });
    }

    [HttpDelete("/api/medical-surveillance/appointments/{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await medical.DeleteAppointmentAsync(id, ct);
        return result.Error is { } error ? Failure(error) : Ok(new { success = true });
    }

    [HttpGet("/api/medical-surveillance/persons")]
    public async Task<IActionResult> GetPersons([FromQuery] string? search, CancellationToken ct) =>
        Ok(await medical.GetPersonOptionsAsync(search, ct));

    [HttpGet("/api/medical-surveillance/work-tasks")]
    public async Task<IActionResult> GetWorkTasks(CancellationToken ct) =>
        Ok(await medical.GetWorkTasksAsync(ct));
}
