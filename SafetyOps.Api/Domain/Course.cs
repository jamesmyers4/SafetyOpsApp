namespace SafetyOps.Api.Domain;

/// <summary>A course in the training catalog, identified by a short code such as <c>ELV-001</c>.</summary>
public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
