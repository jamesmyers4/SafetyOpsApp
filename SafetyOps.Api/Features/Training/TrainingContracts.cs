namespace SafetyOps.Api.Features.Training;

/// <summary>A class as returned by the API. <c>CourseId</c> is the course code (e.g. <c>ELV-001</c>).</summary>
public sealed record TrainingClassDto(int Id, string CourseTitle, string CourseId, DateOnly ClassDate, string Location);

/// <summary>Create/update body. The course is identified by code; any title sent is ignored.</summary>
public sealed record TrainingClassRequest
{
    public string CourseId { get; init; } = string.Empty;
    public DateOnly ClassDate { get; init; }
    public string Location { get; init; } = string.Empty;
}

public sealed record CourseDto(string Id, string Title);
