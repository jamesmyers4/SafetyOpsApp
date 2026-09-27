using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Access;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Personnel;

public interface IPersonnelService
{
    Task<PagedResult<PersonDto>> ListAsync(ListQuery listQuery, CancellationToken ct = default);
    Task<PersonDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PersonOptionDto>> LookupAsync(string? search, CancellationToken ct = default);
    Task<Result<PersonDto>> CreateAsync(PersonRequest request, CancellationToken ct = default);
    Task<Result<PersonDto>> UpdateAsync(int id, PersonRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Personnel records, limited to the org units the signed-in user has a role on.</summary>
public sealed class PersonnelService(AppDbContext db, IAccessScope scope) : IPersonnelService
{
    public static readonly ServiceError NotFound = ServiceError.NotFound("Person not found.");

    public async Task<PagedResult<PersonDto>> ListAsync(ListQuery listQuery, CancellationToken ct = default)
    {
        var query = await VisiblePeopleAsync(ct);
        if (!string.IsNullOrWhiteSpace(listQuery.Search))
        {
            var pattern = SqlLike.Contains(listQuery.Search);
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern, SqlLike.Escape) ||
                EF.Functions.Like(p.LastName, pattern, SqlLike.Escape) ||
                EF.Functions.Like(p.MiddleName, pattern, SqlLike.Escape) ||
                EF.Functions.Like(p.Department, pattern, SqlLike.Escape));
        }
        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ThenBy(p => p.Id)
            .Select(ToDto)
            .ToPagedResultAsync(listQuery, ct);
    }

    public async Task<PersonDto?> GetAsync(int id, CancellationToken ct = default) =>
        await (await VisiblePeopleAsync(ct)).Where(p => p.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PersonOptionDto>> LookupAsync(string? search, CancellationToken ct = default)
    {
        var query = await VisiblePeopleAsync(ct);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.FirstName + " " + p.LastName, SqlLike.Contains(search), SqlLike.Escape));
        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new PersonOptionDto(p.Id, p.FirstName + " " + p.LastName))
            .ToListAsync(ct);
    }

    public async Task<Result<PersonDto>> CreateAsync(PersonRequest request, CancellationToken ct = default)
    {
        var unit = (await scope.GetAsync(ct)).ResolveWriteUnit(request.OrgUnitId);
        if (unit.Error is { } error)
            return error;

        var person = new Person { OrgUnitId = unit.Value };
        Apply(request, person);
        db.People.Add(person);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(person.Id, ct))!;
    }

    public async Task<Result<PersonDto>> UpdateAsync(int id, PersonRequest request, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var person = await db.People.FindAsync([id], ct);
        if (person is null || !access.CanRead(person.OrgUnitId))
            return NotFound;
        if (!access.CanWrite(person.OrgUnitId))
            return AccessRules.ReadOnly;
        var unit = access.ResolveWriteUnit(request.OrgUnitId, person.OrgUnitId);
        if (unit.Error is { } error)
            return error;

        person.OrgUnitId = unit.Value;
        Apply(request, person);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(id, ct))!;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var person = await db.People.FindAsync([id], ct);
        if (person is null || !access.CanRead(person.OrgUnitId))
            return NotFound;
        if (!access.CanWrite(person.OrgUnitId))
            return AccessRules.ReadOnly;
        if (await db.MedicalAppointments.AnyAsync(a => a.PersonId == id, ct))
            return ServiceError.Conflict("This person has medical surveillance appointments and cannot be deleted.");
        if (await db.Incidents.AnyAsync(i => i.ReportedById == id, ct))
            return ServiceError.Conflict("This person has reported incidents and cannot be deleted.");
        db.People.Remove(person);
        await db.SaveChangesAsync(ct);
        return Result.Success;
    }

    private async Task<IQueryable<Person>> VisiblePeopleAsync(CancellationToken ct)
    {
        var readable = (await scope.GetAsync(ct)).UnitsWith(Role.Viewer);
        return db.People.AsNoTracking().Where(p => readable.Contains(p.OrgUnitId));
    }

    private static readonly Expression<Func<Person, PersonDto>> ToDto = p => new PersonDto(
        p.Id, p.FirstName, p.LastName, p.MiddleName, p.Gender, p.Department, p.EmployeeCategory, p.Subscription, p.EmployeeNumber,
        p.OrgUnitId, p.OrgUnit.Name);

    private static void Apply(PersonRequest r, Person p)
    {
        p.FirstName = r.FirstName;
        p.LastName = r.LastName;
        p.MiddleName = r.MiddleName;
        p.Gender = r.Gender;
        p.Department = r.Department;
        p.EmployeeCategory = r.EmployeeCategory;
        p.Subscription = r.Subscription;
        p.EmployeeNumber = r.EmployeeNumber;
    }
}
