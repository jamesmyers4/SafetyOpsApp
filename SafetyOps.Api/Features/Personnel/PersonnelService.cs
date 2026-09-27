using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
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

public sealed class PersonnelService(AppDbContext db) : IPersonnelService
{
    public async Task<PagedResult<PersonDto>> ListAsync(ListQuery listQuery, CancellationToken ct = default)
    {
        var query = db.People.AsNoTracking();
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

    public Task<PersonDto?> GetAsync(int id, CancellationToken ct = default) =>
        db.People.AsNoTracking().Where(p => p.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PersonOptionDto>> LookupAsync(string? search, CancellationToken ct = default)
    {
        var query = db.People.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.FirstName + " " + p.LastName, SqlLike.Contains(search), SqlLike.Escape));
        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new PersonOptionDto(p.Id, p.FirstName + " " + p.LastName))
            .ToListAsync(ct);
    }

    public async Task<Result<PersonDto>> CreateAsync(PersonRequest request, CancellationToken ct = default)
    {
        var person = new Person();
        Apply(request, person);
        db.People.Add(person);
        await db.SaveChangesAsync(ct);
        return MapToDto(person);
    }

    public async Task<Result<PersonDto>> UpdateAsync(int id, PersonRequest request, CancellationToken ct = default)
    {
        var person = await db.People.FindAsync([id], ct);
        if (person is null)
            return Errors.NotFound;
        Apply(request, person);
        await db.SaveChangesAsync(ct);
        return MapToDto(person);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var person = await db.People.FindAsync([id], ct);
        if (person is null)
            return Errors.NotFound;
        if (await db.MedicalAppointments.AnyAsync(a => a.PersonId == id, ct))
            return ServiceError.Conflict("This person has medical surveillance appointments and cannot be deleted.");
        if (await db.Incidents.AnyAsync(i => i.ReportedById == id, ct))
            return ServiceError.Conflict("This person has reported incidents and cannot be deleted.");
        db.People.Remove(person);
        await db.SaveChangesAsync(ct);
        return Result.Success;
    }

    // One mapping, usable both inside EF queries (as an expression) and in memory (compiled).
    private static readonly Expression<Func<Person, PersonDto>> ToDto = p => new PersonDto(
        p.Id, p.FirstName, p.LastName, p.MiddleName, p.Gender, p.Department, p.EmployeeCategory, p.Subscription, p.EmployeeNumber);
    private static readonly Func<Person, PersonDto> MapToDto = ToDto.Compile();

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

    private static class Errors
    {
        public static readonly ServiceError NotFound = ServiceError.NotFound("Person not found.");
    }
}
