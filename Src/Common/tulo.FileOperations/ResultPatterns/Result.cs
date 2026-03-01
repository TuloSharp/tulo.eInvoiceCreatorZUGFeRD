namespace tulo.FileOperations.ResultPatterns;

/// <summary>
/// Represents the result of an operation, which may succeed or fail.
/// Contains either a value (on success) or an error (on failure).
/// </summary>
public sealed class Result<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The value returned by the operation, if successful.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// The error associated with a failed operation.
    /// </summary>
    public Error Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result containing the given value.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Implicitly converts a value into a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error into a failed result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Applies the appropriate function based on success or failure.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error);
}

/// <summary>
/// Represents a non-generic result for operations that return no value.
/// </summary>
public sealed class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error associated with a failed operation.
    /// </summary>
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents a structured error with a message and optional code.
/// </summary>
public struct Error
{
    /// <summary>
    /// The error message describing what went wrong.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Optional error code for more detailed categorization.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static Error None => new("No error");

    /// <summary>
    /// Creates a new error instance.
    /// </summary>
    public Error(string message, string? code = null)
    {
        Message = message;
        Code = code;
    }

    /// <summary>
    /// Returns a string representation of the object, including the code (if available) and the message.
    /// </summary>
    /// <returns>
    /// A string in the format "Code: Message" if <c>Code</c> is not null; otherwise, just the <c>Message</c>.
    /// </returns>
    public override string ToString() => Code != null ? $"{Code}: {Message}" : Message;
}