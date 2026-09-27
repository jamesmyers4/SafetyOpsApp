namespace SafetyOps.Api.Features.Personnel;

public sealed record PersonDto(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Gender,
    string Department,
    string EmployeeCategory,
    string Subscription,
    string EmployeeNumber);

/// <summary>Create/update body. Has no <c>Id</c>, so clients can't choose or overwrite keys.</summary>
public sealed record PersonRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MiddleName { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string EmployeeCategory { get; init; } = string.Empty;
    public string Subscription { get; init; } = string.Empty;
    public string EmployeeNumber { get; init; } = string.Empty;
}
