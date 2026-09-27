using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Incidents;

public interface IIncidentService
{
    Task<PagedResult<IncidentDto>> ListAsync(IncidentListQuery listQuery, CancellationToken ct = default);
    Task<IncidentDto?> GetAsync(int id, CancellationToken ct = default);
    Task<Result<IncidentDto>> CreateAsync(IncidentRequest request, CancellationToken ct = default);
    Task<Result<IncidentDto>> UpdateAsync(int id, IncidentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class IncidentService(AppDbContext db, TimeProvider clock) : IIncidentService
{
    public static readonly ServiceError NotFound = ServiceError.NotFound("Incident not found.");

    public async Task<PagedResult<IncidentDto>> ListAsync(IncidentListQuery listQuery, CancellationToken ct = default)
    {
        var query = db.Incidents.AsNoTracking();
        if (listQuery.Status is { } status)
            query = query.Where(i => i.Status == status);
        if (listQuery.Category is { } category)
            query = query.Where(i => i.Category == category);
        if (!string.IsNullOrWhiteSpace(listQuery.Search))
        {
            var pattern = SqlLike.Contains(listQuery.Search);
            query = query.Where(i =>
                EF.Functions.Like(i.Location, pattern, SqlLike.Escape) ||
                EF.Functions.Like(i.Description, pattern, SqlLike.Escape) ||
                EF.Functions.Like(i.ReportedBy.FirstName + " " + i.ReportedBy.LastName, pattern, SqlLike.Escape));
        }
        return await query
            .OrderByDescending(i => i.OccurredAt).ThenByDescending(i => i.Id)
            .Select(ToDto)
            .ToPagedResultAsync(listQuery.ToListQuery(), ct);
    }

    public Task<IncidentDto?> GetAsync(int id, CancellationToken ct = default) =>
        db.Incidents.AsNoTracking().Where(i => i.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<Result<IncidentDto>> CreateAsync(IncidentRequest request, CancellationToken ct = default)
    {
        var incident = new Incident();
        if (await ApplyAsync(request, incident, ct) is { } error)
            return error;
        db.Incidents.Add(incident);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(incident.Id, ct))!;
    }

    public async Task<Result<IncidentDto>> UpdateAsync(int id, IncidentRequest request, CancellationToken ct = default)
    {
        var incident = await db.Incidents.FindAsync([id], ct);
        if (incident is null)
            return NotFound;
        if (await ApplyAsync(request, incident, ct) is { } error)
            return error;
        await db.SaveChangesAsync(ct);
        return (await GetAsync(id, ct))!;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        await db.Incidents.Where(i => i.Id == id).ExecuteDeleteAsync(ct) > 0 ? Result.Success : NotFound;

    private async Task<ServiceError?> ApplyAsync(IncidentRequest request, Incident incident, CancellationToken ct)
    {
        // Minute precision: the form captures hours and minutes only.
        var occurredAt = request.OccurredAt!.Value;
        occurredAt = new DateTime(occurredAt.Year, occurredAt.Month, occurredAt.Day, occurredAt.Hour, occurredAt.Minute, 0, DateTimeKind.Unspecified);
        if (occurredAt > clock.GetLocalNow().DateTime)
            return ServiceError.Invalid("occurredAt", "The incident can't be in the future.");
        if (!await db.People.AnyAsync(p => p.Id == request.ReportedById, ct))
            return ServiceError.Invalid("reportedById", $"Unknown person {request.ReportedById}.");

        incident.OccurredAt = occurredAt;
        incident.Location = request.Location.Trim();
        incident.Category = request.Category!.Value;
        incident.Severity = request.Severity!.Value;
        incident.Description = request.Description.Trim();
        incident.ReportedById = request.ReportedById;
        incident.Status = request.Status ?? (incident.Id == 0 ? IncidentStatus.Open : incident.Status);
        return null;
    }

    private static readonly Expression<Func<Incident, IncidentDto>> ToDto = i => new IncidentDto(
        i.Id, i.OccurredAt, i.Location, i.Category, i.Severity, i.Description,
        i.ReportedById, i.ReportedBy.FirstName + " " + i.ReportedBy.LastName, i.Status);
}
