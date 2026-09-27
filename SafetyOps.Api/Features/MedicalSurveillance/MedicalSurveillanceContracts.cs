using System.ComponentModel.DataAnnotations;

namespace SafetyOps.Api.Features.MedicalSurveillance;

/// <summary>
/// A medical surveillance appointment. Stressors are identified by code (e.g. <c>STR-001</c>).
/// The org unit is the person's; access to the appointment follows it.
/// </summary>
public sealed record AppointmentDto(int Id, DateOnly Date, int PersonId, string PersonName, IReadOnlyList<AppointmentStressorDto> Stressors, int OrgUnitId, string OrgUnitName);

/// <summary>A stressor evaluated at an appointment and the exam type used.</summary>
public sealed record AppointmentStressorDto(string StressorId, string StressorName, string ExamType);

/// <summary>Create/update body.</summary>
public sealed record AppointmentRequest
{
    /// <summary>Appointment date (ISO 8601).</summary>
    [Required]
    public DateOnly? Date { get; init; }

    /// <summary>Id of the person evaluated, from <c>GET /api/medical-surveillance/persons</c>.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Person evaluated is required.")]
    public int PersonId { get; init; }

    /// <summary>Stressors evaluated. Each stressor may appear once.</summary>
    [MaxLength(20)]
    public IReadOnlyList<AppointmentStressorRequest> Stressors { get; init; } = [];
}

/// <summary>A stressor to record on an appointment.</summary>
public sealed record AppointmentStressorRequest
{
    /// <summary>Stressor code from <c>GET /api/medical-surveillance/work-tasks</c>.</summary>
    [Required(AllowEmptyStrings = false), StringLength(20)]
    public string StressorId { get; init; } = string.Empty;

    [StringLength(50)]
    public string ExamType { get; init; } = string.Empty;
}

/// <summary>A work task, the stressors it involves, and the exam types that apply.</summary>
public sealed record WorkTaskDto(string Id, string Name, IReadOnlyList<WorkTaskStressorDto> Stressors, IReadOnlyList<string> ExamTypeOptions);

/// <summary>A stressor linked to a work task.</summary>
public sealed record WorkTaskStressorDto(string StressorId, string StressorName);
