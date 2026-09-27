using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Training;

public interface ITrainingService
{
    Task<IReadOnlyList<TrainingClassDto>> SearchClassesAsync(string? search, CancellationToken ct = default);
    Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default);
    Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default);
    Task<Result> DeleteClassAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(string? search, CancellationToken ct = default);
}

public sealed class TrainingService(AppDbContext db) : ITrainingService
{
    private static readonly Error ClassNotFound = Error.NotFound("Class not found");

    public async Task<IReadOnlyList<TrainingClassDto>> SearchClassesAsync(string? search, CancellationToken ct = default)
    {
        var query = db.TrainingClasses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = SqlLike.Contains(search);
            query = query.Where(c =>
                EF.Functions.Like(c.Course.Title, pattern, SqlLike.Escape) ||
                EF.Functions.Like(c.Location, pattern, SqlLike.Escape));
        }
        return await query.OrderByDescending(c => c.Id).Select(ToDto).ToListAsync(ct);
    }

    public Task<TrainingClassDto?> GetClassAsync(int id, CancellationToken ct = default) =>
        db.TrainingClasses.AsNoTracking().Where(c => c.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

    public async Task<Result<TrainingClassDto>> CreateClassAsync(TrainingClassRequest request, CancellationToken ct = default)
    {
        var course = await FindCourseAsync(request.CourseId, ct);
        if (course is null)
            return UnknownCourse(request.CourseId);

        var cls = new TrainingClass { Course = course, ClassDate = request.ClassDate, Location = request.Location };
        db.TrainingClasses.Add(cls);
        await db.SaveChangesAsync(ct);
        return MapToDto(cls);
    }

    public async Task<Result<TrainingClassDto>> UpdateClassAsync(int id, TrainingClassRequest request, CancellationToken ct = default)
    {
        var cls = await db.TrainingClasses.FindAsync([id], ct);
        if (cls is null)
            return ClassNotFound;
        var course = await FindCourseAsync(request.CourseId, ct);
        if (course is null)
            return UnknownCourse(request.CourseId);

        cls.Course = course;
        cls.ClassDate = request.ClassDate;
        cls.Location = request.Location;
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

    private Task<Course?> FindCourseAsync(string code, CancellationToken ct) =>
        db.Courses.FirstOrDefaultAsync(c => c.Code == code, ct);

    private static Error UnknownCourse(string code) => Error.Invalid("courseId", $"Unknown course '{code}'");

    private static readonly Expression<Func<TrainingClass, TrainingClassDto>> ToDto = c =>
        new TrainingClassDto(c.Id, c.Course.Title, c.Course.Code, c.ClassDate, c.Location);
    private static readonly Func<TrainingClass, TrainingClassDto> MapToDto = ToDto.Compile();
}
