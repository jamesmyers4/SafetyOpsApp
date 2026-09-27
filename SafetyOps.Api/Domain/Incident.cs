namespace SafetyOps.Api.Domain;

/// <summary>A workplace safety incident: what happened, where, how serious, and who reported it.</summary>
public class Incident
{
    public int Id { get; set; }

    /// <summary>Local wall-clock time at the site when the incident happened.</summary>
    public DateTime OccurredAt { get; set; }

    public string Location { get; set; } = string.Empty;
    public IncidentCategory Category { get; set; }
    public IncidentSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ReportedById { get; set; }
    public Person ReportedBy { get; set; } = null!;
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
}

public enum IncidentCategory
{
    Fire,
    Injury,
    NearMiss,
    PropertyDamage,
    Environmental,
    Other,
}

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical,
}

public enum IncidentStatus
{
    Open,
    UnderReview,
    Closed,
}
