namespace Korp.SharedKernel.Results;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error ServiceUnavailable(string code, string description) =>
        new(code, description, ErrorType.ServiceUnavailable);

    public static Error Unexpected(string code, string description) =>
        new(code, description, ErrorType.Unexpected);
}
