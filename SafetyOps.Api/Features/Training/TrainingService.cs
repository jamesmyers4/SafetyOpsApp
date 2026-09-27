using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Training;

public interface ITrainingService
{
    Task<PagedResult<TrainingClassDto>> ListClassesAsync(ListQuery query, CancellationToken ct = default);
    Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default);
    Task<Result> DeleteClassAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(string? search, CancellationToken ct = default);
}

public sealed class TrainingService(AppDbContext db, TimeProvider clock) : ITrainingService
{
    public static readonly Error ClassNotFound = Error.NotFound("Class not found.");

    public async Task<PagedResult<TrainingClassDto>> ListClassesAsync(ListQuery list, CancellationToken ct = default)
    {
        var query = db.TrainingClasses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(list.Search))
        {
            var pattern = SqlLike.Contains(list.Search);
            query = query.Where(c =>
                EF.Functions.Like(c.Course.Title, pattern, SqlLike.Escape) ||
                EF.Functions.Like(c.Location, pattern, SqlLike.Escape));
        }
        return await query.OrderByDescending(c => c.Id).Select(ToDto).ToPagedResultAsync(list, ct);
    }

    public Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default) =>
        db.TrainingClasses.AsNoTracking().Where(c => c.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default)
    {
        var cls = new TrainingClass();
        if (await ApplyAsync(request, cls, ct) is { } error)
            return error;
        db.TrainingClasses.Add(cls);
        await db.SaveChangesAsync(ct);
        return MapToDto(cls);
    }

    public async Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default)
    {
        var cls = await db.TrainingClasses.FindAsync([id], ct);
        if (cls is null)
            return ClassNotFound;
        if (await ApplyAsync(request, cls, ct) is { } error)
            return error;
        await db.SaveChangesAsync(ct);
        return MapToDto(cls);
    }

    public async Task<Result> DeleteClassAsync(int id, CancellationToken ct = default) =>
        await db.TrainingClasses.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0
            ? Result.Success
            : ClassNotFound;

    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(string? search, CancellationToken ct = default)
    {
        var query = db.Courses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.Like(c.Title, SqlLike.Contains(search), SqlLike.Escape));
        return await query.OrderBy(c => c.Id).Select(c => new CourseDto(c.Code, c.Title)).ToListAsync(ct);
    }

    private async Task<Error?> ApplyAsync(TrainingClassRequest request, TrainingClass cls, CancellationToken ct)
    {
        var date = request.ClassDate!.Value;
        if (date > DateOnly.FromDateTime(clock.GetLocalNow().DateTime))
            return Error.Invalid("classDate", "Future dates are not allowed.");

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Code == request.CourseId, ct);
        if (course is null)
            return Error.Invalid("courseId", $"Unknown course '{request.CourseId}'.");

        cls.Course = course;
        cls.ClassDate = date;
        cls.Location = request.Location.Trim();
        return null;
    }

    private static readonly Expression<Func<TrainingClass, TrainingClassDto>> ToDto = c =>
        new TrainingClassDto(c.Id, c.Course.Title, c.Course.Code, c.ClassDate, c.Location);
    private static readonly Func<TrainingClass, TrainingClassDto> MapToDto = ToDto.Compile();
}
