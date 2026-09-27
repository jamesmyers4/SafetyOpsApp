namespace SafetyOps.Api.Features.Common;

public enum ErrorKind
{
    NotFound,
    Invalid,
    Conflict,
    Forbidden,
}

/// <summary>An expected failure a service reports instead of throwing. Controllers turn it into an HTTP response.</summary>
public sealed record ServiceError(ErrorKind Kind, string Message, string? Field = null)
{
    public static ServiceError NotFound(string message) => new(ErrorKind.NotFound, message);
    public static ServiceError Invalid(string field, string message) => new(ErrorKind.Invalid, message, field);
    public static ServiceError Conflict(string message) => new(ErrorKind.Conflict, message);
    public static ServiceError Forbidden(string message) => new(ErrorKind.Forbidden, message);
}

/// <summary>Either a value or an <see cref="ServiceError"/>.</summary>
public readonly record struct Result<T>
{
    private Result(T? value, ServiceError? error) => (Value, Error) = (value, error);

    public T? Value { get; }
    public ServiceError? Error { get; }

    public static implicit operator Result<T>(T value) => new(value, null);
    public static implicit operator Result<T>(ServiceError error) => new(default, error);
}

/// <summary>Success, or an <see cref="ServiceError"/>.</summary>
public readonly record struct Result
{
    private Result(ServiceError? error) => Error = error;

    public ServiceError? Error { get; }

    public static Result Success => new(null);

    public static implicit operator Result(ServiceError error) => new(error);
}
