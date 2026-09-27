using System.ComponentModel.DataAnnotations;

namespace SafetyOps.Api.Features.Auth;

/// <summary>Sign-in body.</summary>
public sealed record LoginRequest
{
    [Required(AllowEmptyStrings = false), StringLength(50)]
    public string Username { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false), StringLength(200)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>The signed-in user.</summary>
public sealed record CurrentUserDto(int Id, string UserName, string DisplayName);
