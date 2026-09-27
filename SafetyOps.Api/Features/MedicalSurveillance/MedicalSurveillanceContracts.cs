namespace SafetyOps.Api.Features.MedicalSurveillance;

/// <summary>An appointment as returned by the API. Stressors are identified by code (e.g. <c>STR-001</c>).</summary>
public sealed record AppointmentDto(int Id, DateOnly Date, int PersonId, string PersonName, IReadOnlyList<AppointmentStressorDto> Stressors);

public sealed record AppointmentStressorDto(string StressorId, string StressorName, string ExamType);

/// <summary>Create/update body. Names sent by the client are ignored in favor of the stored person and stressors.</summary>
public sealed record AppointmentRequest
{
    public DateOnly Date { get; init; }
    public int PersonId { get; init; }
    public IReadOnlyList<AppointmentStressorRequest> Stressors { get; init; } = [];
}

public sealed record AppointmentStressorRequest
{
    public string StressorId { get; init; } = string.Empty;
    public string ExamType { get; init; } = string.Empty;
}

public sealed record PersonOptionDto(int Id, string Name);

public sealed record WorkTaskDto(string Id, string Name, IReadOnlyList<WorkTaskStressorDto> Stressors, IReadOnlyList<string> ExamTypeOptions);

public sealed record WorkTaskStressorDto(string StressorId, string StressorName);
