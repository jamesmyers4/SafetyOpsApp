namespace SafetyOps.Api.Domain;

/// <summary>A medical surveillance exam for a <see cref="Person"/>, covering one or more stressors.</summary>
public class MedicalAppointment
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public List<AppointmentStressor> Stressors { get; set; } = [];
}

/// <summary>A stressor evaluated during an appointment, with the exam type used.</summary>
public class AppointmentStressor
{
    public int AppointmentId { get; set; }
    public int StressorId { get; set; }
    public Stressor Stressor { get; set; } = null!;
    public string ExamType { get; set; } = string.Empty;
}
