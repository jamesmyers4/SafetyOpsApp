using System.ComponentModel.DataAnnotations;

namespace SafetyOps.Api.Features.Training;

/// <summary>A training class. <c>CourseId</c> is the course code (e.g. <c>ELV-001</c>).</summary>
public sealed record TrainingClassDto(int Id, string CourseTitle, string CourseId, DateOnly ClassDate, string Location);

/// <summary>Create/update body. The course is identified by its code.</summary>
public sealed record TrainingClassRequest
{
    /// <summary>Course code from <c>GET /api/training/courses</c>.</summary>
    [Required(AllowEmptyStrings = false), StringLength(20)]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>Date the class was held (ISO 8601). Future dates are rejected.</summary>
    [Required]
    public DateOnly? ClassDate { get; init; }

    [Required(AllowEmptyStrings = false), StringLength(200)]
    public string Location { get; init; } = string.Empty;
}

/// <summary>A course in the training catalog.</summary>
public sealed record CourseDto(string Id, string Title);
