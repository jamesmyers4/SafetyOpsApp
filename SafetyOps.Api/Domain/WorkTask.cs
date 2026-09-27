namespace SafetyOps.Api.Domain;

/// <summary>A job task, the stressors it exposes workers to, and the exam types that apply.</summary>
public class WorkTask
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Stressor> Stressors { get; set; } = [];
    public List<string> ExamTypeOptions { get; set; } = [];
}
