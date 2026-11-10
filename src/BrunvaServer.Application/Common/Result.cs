namespace BrunvaServer.Application.Common;

public static class HttpStatus
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int NoContent = 204;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
}

public interface IStatusResult
{
    int StatusCode { get; }
    bool IsSuccess { get; }
    object? Body { get; }
    string? Code { get; }
    string? Title { get; }
    string? Detail { get; }
    IReadOnlyDictionary<string, string[]>? Errors { get; }
}

public interface IResultFactory<TSelf>
    where TSelf : IResultFactory<TSelf>, IStatusResult
{
    static abstract TSelf Success(int statusCode, object? body);
    static abstract TSelf Failure(int statusCode, string code, string title, string detail);
    static abstract TSelf Validation(IReadOnlyDictionary<string, string[]> errors);
}

public sealed record Result<T> : IStatusResult, IResultFactory<Result<T>>
{
    public int StatusCode { get; private init; } = HttpStatus.Ok;

    public T? Value { get; private init; }

    public string? Code { get; private init; }

    public string? Title { get; private init; }

    public string? Detail { get; private init; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; private init; }

    public bool IsSuccess => StatusCode is >= 200 and < 300;

    public object? Body => StatusCode == HttpStatus.NoContent ? null : Value;

    public static Result<T> Ok(T value) => Success(HttpStatus.Ok, value);

    public static Result<T> Created(T value) => Success(HttpStatus.Created, value);

    public static Result<T> NoContent() => Success(HttpStatus.NoContent, null);

    public static Result<T> Success(int statusCode, object? body) =>
        new()
        {
            StatusCode = statusCode,
            Value = body is T value ? value : default
        };

    public static Result<T> Failure(int statusCode, string code, string title, string detail) =>
        new()
        {
            StatusCode = statusCode,
            Code = code,
            Title = title,
            Detail = detail
        };

    public static Result<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new()
        {
            StatusCode = HttpStatus.BadRequest,
            Code = "validation_failed",
            Title = "Validation failed",
            Detail = "One or more fields are invalid.",
            Errors = errors
        };
}

public interface ITransactionalRequest;

public sealed class EmptyBody;

public sealed class ConflictException : Exception
{
    public ConflictException(string code, string title, string detail)
        : base(detail)
    {
        Code = code;
        Title = title;
        Detail = detail;
    }

    public string Code { get; }

    public string Title { get; }

    public string Detail { get; }

    public static ConflictException Duplicate() =>
        new("conflict", "Conflict", "The record already exists.");

    public static ConflictException Concurrency() =>
        new("conflict", "Conflict", "The record was modified by someone else.");
}
