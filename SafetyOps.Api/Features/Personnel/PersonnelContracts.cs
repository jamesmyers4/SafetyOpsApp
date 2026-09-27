using System.ComponentModel.DataAnnotations;

namespace SafetyOps.Api.Features.Personnel;

/// <summary>A personnel record.</summary>
public sealed record PersonDto(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Gender,
    string Department,
    string EmployeeCategory,
    string Subscription,
    string EmployeeNumber,
    int OrgUnitId,
    string OrgUnitName);

/// <summary>Create/update body. Has no <c>Id</c>, so clients can't choose or overwrite keys.</summary>
public sealed record PersonRequest
{
    [Required(AllowEmptyStrings = false), StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false), StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [StringLength(100)]
    public string MiddleName { get; init; } = string.Empty;

    [StringLength(50)]
    public string Gender { get; init; } = string.Empty;

    [StringLength(100)]
    public string Department { get; init; } = string.Empty;

    [StringLength(50)]
    public string EmployeeCategory { get; init; } = string.Empty;

    [StringLength(50)]
    public string Subscription { get; init; } = string.Empty;

    [StringLength(20), RegularExpression("^[0-9]*$", ErrorMessage = "Employee number must contain digits only.")]
    public string EmployeeNumber { get; init; } = string.Empty;

    /// <summary>Owning org unit. On create defaults to the highest unit you can write to; on update defaults to the current one.</summary>
    [Range(1, int.MaxValue)]
    public int? OrgUnitId { get; init; }
}

/// <summary>A person as a pick-list option: id and full name.</summary>
public sealed record PersonOptionDto(int Id, string Name);
