namespace SafetyOps.Api.Domain;

/// <summary>A scheduled session of a <see cref="Course"/> at a location.</summary>
public class TrainingClass
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateOnly ClassDate { get; set; }
    public string Location { get; set; } = string.Empty;
}
