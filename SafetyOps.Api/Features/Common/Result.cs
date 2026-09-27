namespace SafetyOps.Api.Features.Common;

public enum ErrorKind
{
    NotFound,
    Invalid,
    Conflict,
}

/// <summary>An expected failure a service reports instead of throwing. Controllers turn it into an HTTP response.</summary>
public sealed record Error(ErrorKind Kind, string Message, string? Field = null)
{
    public static Error NotFound(string message) => new(ErrorKind.NotFound, message);
    public static Error Invalid(string field, string message) => new(ErrorKind.Invalid, message, field);
    public static Error Conflict(string message) => new(ErrorKind.Conflict, message);
}

/// <summary>Either a value or an <see cref="Common.Error"/>.</summary>
public readonly record struct Result<T>
{
    private Result(T? value, Error? error) => (Value, Error) = (value, error);

    public T? Value { get; }
    public Error? Error { get; }

    public static implicit operator Result<T>(T value) => new(value, null);
    public static implicit operator Result<T>(Error error) => new(default, error);
}

/// <summary>Success, or an <see cref="Common.Error"/>.</summary>
public readonly record struct Result
{
    private Result(Error? error) => Error = error;

    public Error? Error { get; }

    public static Result Success => new(null);

    public static implicit operator Result(Error error) => new(error);
}
