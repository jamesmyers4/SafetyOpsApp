using System.ComponentModel.DataAnnotations;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Features.Incidents;

/// <summary>An incident report.</summary>
public sealed record IncidentDto(
    int Id,
    DateTime OccurredAt,
    string Location,
    IncidentCategory Category,
    IncidentSeverity Severity,
    string Description,
    int ReportedById,
    string ReportedByName,
    IncidentStatus Status);

/// <summary>Create/update body.</summary>
public sealed record IncidentRequest
{
    /// <summary>Local time at the site (<c>yyyy-MM-ddTHH:mm</c>). Future times are rejected.</summary>
    [Required]
    public DateTime? OccurredAt { get; init; }

    [Required(AllowEmptyStrings = false), StringLength(200)]
    public string Location { get; init; } = string.Empty;

    [Required, EnumDataType(typeof(IncidentCategory))]
    public IncidentCategory? Category { get; init; }

    [Required, EnumDataType(typeof(IncidentSeverity))]
    public IncidentSeverity? Severity { get; init; }

    [Required(AllowEmptyStrings = false), StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    /// <summary>Id of the person reporting, from <c>GET /api/personnel/lookup</c>.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Reported by is required.")]
    public int ReportedById { get; init; }

    /// <summary>Defaults to Open on create.</summary>
    [EnumDataType(typeof(IncidentStatus))]
    public IncidentStatus? Status { get; init; }
}

/// <summary>Query for the incident list: search/paging plus optional filters.</summary>
public sealed record IncidentListQuery
{
    [StringLength(100)]
    public string? Search { get; init; }

    [EnumDataType(typeof(IncidentStatus))]
    public IncidentStatus? Status { get; init; }

    [EnumDataType(typeof(IncidentCategory))]
    public IncidentCategory? Category { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, ListQuery.MaxPageSize)]
    public int PageSize { get; init; } = 25;

    public ListQuery ToListQuery() => new() { Search = Search, Page = Page, PageSize = PageSize };
}
