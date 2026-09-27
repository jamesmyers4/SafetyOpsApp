namespace SafetyOps.Api.Domain;

/// <summary>A scheduled session of a <see cref="Course"/> at a location.</summary>
public class TrainingClass
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateOnly ClassDate { get; set; }
    public string Location { get; set; } = string.Empty;

    /// <summary>Owning org unit; access to this record follows roles on that unit and its ancestors.</summary>
    public int OrgUnitId { get; set; }
    public OrgUnit OrgUnit { get; set; } = null!;
}
