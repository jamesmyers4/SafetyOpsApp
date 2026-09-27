namespace SafetyOps.Api.Domain;

/// <summary>A workplace exposure (noise, solvents, dust, ...) that calls for medical surveillance.</summary>
public class Stressor
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
