using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.MedicalSurveillance;

public interface IMedicalSurveillanceService
{
    Task<PagedResult<AppointmentDto>> ListAppointmentsAsync(ListQuery query, CancellationToken ct = default);
    Task<AppointmentDto?> GetAppointmentAsync(int id, CancellationToken ct = default);
    Task<Result<AppointmentDto>> CreateAppointmentAsync(AppointmentRequest request, CancellationToken ct = default);
    Task<Result<AppointmentDto>> UpdateAppointmentAsync(int id, AppointmentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAppointmentAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PersonOptionDto>> GetPersonOptionsAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<WorkTaskDto>> GetWorkTasksAsync(CancellationToken ct = default);
}

public sealed class MedicalSurveillanceService(AppDbContext db) : IMedicalSurveillanceService
{
    public static readonly Error AppointmentNotFound = Error.NotFound("Appointment not found.");

    public async Task<PagedResult<AppointmentDto>> ListAppointmentsAsync(ListQuery list, CancellationToken ct = default)
    {
        var query = db.MedicalAppointments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(list.Search))
        {
            // A search term can be part of the person's name, an appointment date, or an appointment id.
            var term = list.Search.Trim();
            var pattern = SqlLike.Contains(term);
            var hasDate = DateFormats.TryParse(term, out var date);
            var hasId = int.TryParse(term, out var id);
            query = query.Where(a =>
                EF.Functions.Like(a.Person.FirstName + " " + a.Person.LastName, pattern, SqlLike.Escape) ||
                (hasDate && a.Date == date) ||
                (hasId && a.Id == id));
        }
        return await query.OrderByDescending(a => a.Id).Select(ToDto).ToPagedResultAsync(list, ct);
    }

    public Task<AppointmentDto?> GetAppointmentAsync(int id, CancellationToken ct = default) =>
        db.MedicalAppointments.AsNoTracking().Where(a => a.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<Result<AppointmentDto>> CreateAppointmentAsync(AppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = new MedicalAppointment();
        if (await ApplyAsync(request, appointment, ct) is { } error)
            return error;
        db.MedicalAppointments.Add(appointment);
        await db.SaveChangesAsync(ct);
        return (await GetAppointmentAsync(appointment.Id, ct))!;
    }

    public async Task<Result<AppointmentDto>> UpdateAppointmentAsync(int id, AppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = await db.MedicalAppointments.Include(a => a.Stressors).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appointment is null)
            return AppointmentNotFound;
        if (await ApplyAsync(request, appointment, ct) is { } error)
            return error;
        await db.SaveChangesAsync(ct);
        return (await GetAppointmentAsync(id, ct))!;
    }

    public async Task<Result> DeleteAppointmentAsync(int id, CancellationToken ct = default)
    {
        var appointment = await db.MedicalAppointments.Include(a => a.Stressors).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appointment is null)
            return AppointmentNotFound;
        db.MedicalAppointments.Remove(appointment);
        await db.SaveChangesAsync(ct);
        return Result.Success;
    }

    public async Task<IReadOnlyList<PersonOptionDto>> GetPersonOptionsAsync(string? search, CancellationToken ct = default)
    {
        var query = db.People.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.FirstName + " " + p.LastName, SqlLike.Contains(search), SqlLike.Escape));
        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new PersonOptionDto(p.Id, p.FirstName + " " + p.LastName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WorkTaskDto>> GetWorkTasksAsync(CancellationToken ct = default) =>
        await db.WorkTasks.AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new WorkTaskDto(
                t.Code,
                t.Name,
                t.Stressors.OrderBy(s => s.Id).Select(s => new WorkTaskStressorDto(s.Code, s.Name)).ToList(),
                t.ExamTypeOptions))
            .ToListAsync(ct);

    // Resolves the person and stressor codes, then copies the request onto the appointment.
    private async Task<Error?> ApplyAsync(AppointmentRequest request, MedicalAppointment appointment, CancellationToken ct)
    {
        if (!await db.People.AnyAsync(p => p.Id == request.PersonId, ct))
            return Error.Invalid("personId", $"Unknown person {request.PersonId}.");

        var duplicate = request.Stressors.GroupBy(s => s.StressorId).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            return Error.Invalid("stressors", $"Stressor '{duplicate.Key}' is listed more than once.");

        var codes = request.Stressors.Select(s => s.StressorId).Distinct().ToList();
        var stressorIds = await db.Stressors.Where(s => codes.Contains(s.Code)).ToDictionaryAsync(s => s.Code, s => s.Id, ct);
        if (codes.FirstOrDefault(c => !stressorIds.ContainsKey(c)) is { } unknown)
            return Error.Invalid("stressors", $"Unknown stressor '{unknown}'.");

        appointment.Date = request.Date!.Value;
        appointment.PersonId = request.PersonId;
        appointment.Stressors.Clear();
        foreach (var s in request.Stressors)
            appointment.Stressors.Add(new AppointmentStressor { StressorId = stressorIds[s.StressorId], ExamType = s.ExamType });
        return null;
    }

    private static readonly Expression<Func<MedicalAppointment, AppointmentDto>> ToDto = a => new AppointmentDto(
        a.Id,
        a.Date,
        a.PersonId,
        a.Person.FirstName + " " + a.Person.LastName,
        a.Stressors
            .OrderBy(s => s.StressorId)
            .Select(s => new AppointmentStressorDto(s.Stressor.Code, s.Stressor.Name, s.ExamType))
            .ToList());
}
