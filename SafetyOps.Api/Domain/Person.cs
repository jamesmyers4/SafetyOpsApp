namespace SafetyOps.Api.Domain;

/// <summary>A person in the organization. Personnel records and medical surveillance both refer to this.</summary>
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmployeeCategory { get; set; } = string.Empty;
    public string Subscription { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
