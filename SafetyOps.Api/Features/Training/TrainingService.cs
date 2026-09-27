using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Access;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Training;

public interface ITrainingService
{
    Task<PagedResult<TrainingClassDto>> ListClassesAsync(ListQuery listQuery, CancellationToken ct = default);
    Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default);
    Task<Result> DeleteClassAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(string? search, CancellationToken ct = default);
}

/// <summary>Training classes, limited to the org units the signed-in user has a role on. The course catalog is shared.</summary>
public sealed class TrainingService(AppDbContext db, TimeProvider clock, IAccessScope scope) : ITrainingService
{
    public static readonly ServiceError ClassNotFound = ServiceError.NotFound("Class not found.");

    public async Task<PagedResult<TrainingClassDto>> ListClassesAsync(ListQuery listQuery, CancellationToken ct = default)
    {
        var query = await VisibleClassesAsync(ct);
        if (!string.IsNullOrWhiteSpace(listQuery.Search))
        {
            var pattern = SqlLike.Contains(listQuery.Search);
            query = query.Where(c =>
                EF.Functions.Like(c.Course.Title, pattern, SqlLike.Escape) ||
                EF.Functions.Like(c.Location, pattern, SqlLike.Escape));
        }
        return await query.OrderByDescending(c => c.Id).Select(ToDto).ToPagedResultAsync(listQuery, ct);
    }

    public async Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default) =>
        await (await VisibleClassesAsync(ct)).Where(c => c.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default)
    {
        var unit = (await scope.GetAsync(ct)).ResolveWriteUnit(request.OrgUnitId);
        if (unit.Error is { } denied)
            return denied;
        var cls = new TrainingClass { OrgUnitId = unit.Value };
        if (await ApplyAsync(request, cls, ct) is { } error)
            return error;
        db.TrainingClasses.Add(cls);
        await db.SaveChangesAsync(ct);
        return (await GetClassAsync(cls.Id, ct))!;
    }

    public async Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var cls = await db.TrainingClasses.FindAsync([id], ct);
        if (cls is null || !access.CanRead(cls.OrgUnitId))
            return ClassNotFound;
        if (!access.CanWrite(cls.OrgUnitId))
            return AccessRules.ReadOnly;
        var unit = access.ResolveWriteUnit(request.OrgUnitId, cls.OrgUnitId);
        if (unit.Error is { } denied)
            return denied;
        cls.OrgUnitId = unit.Value;
        if (await ApplyAsync(request, cls, ct) is { } error)
            return error;
        await db.SaveChangesAsync(ct);
        return (await GetClassAsync(id, ct))!;
    }

    public async Task<Result> DeleteClassAsync(int id, CancellationToken ct = default)
    {
        var access = await scope.GetAsync(ct);
        var cls = await db.TrainingClasses.FindAsync([id], ct);
        if (cls is null || !access.CanRead(cls.OrgUnitId))
            return ClassNotFound;
        if (!access.CanWrite(cls.OrgUnitId))
            return AccessRules.ReadOnly;
        db.TrainingClasses.Remove(cls);
        await db.SaveChangesAsync(ct);
        return Result.Success;
    }

    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(string? search, CancellationToken ct = default)
    {
        var query = db.Courses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.Like(c.Title, SqlLike.Contains(search), SqlLike.Escape));
        return await query.OrderBy(c => c.Id).Select(c => new CourseDto(c.Code, c.Title)).ToListAsync(ct);
    }

    private async Task<ServiceError?> ApplyAsync(TrainingClassRequest request, TrainingClass cls, CancellationToken ct)
    {
        var date = request.ClassDate!.Value;
        if (date > DateOnly.FromDateTime(clock.GetLocalNow().DateTime))
            return ServiceError.Invalid("classDate", "Future dates are not allowed.");

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Code == request.CourseId, ct);
        if (course is null)
            return ServiceError.Invalid("courseId", $"Unknown course '{request.CourseId}'.");

        cls.Course = course;
        cls.ClassDate = date;
        cls.Location = request.Location.Trim();
        return null;
    }

    private async Task<IQueryable<TrainingClass>> VisibleClassesAsync(CancellationToken ct)
    {
        var readable = (await scope.GetAsync(ct)).UnitsWith(Role.Viewer);
        return db.TrainingClasses.AsNoTracking().Where(c => readable.Contains(c.OrgUnitId));
    }

    private static readonly Expression<Func<TrainingClass, TrainingClassDto>> ToDto = c =>
        new TrainingClassDto(c.Id, c.Course.Title, c.Course.Code, c.ClassDate, c.Location, c.OrgUnitId, c.OrgUnit.Name);
}
