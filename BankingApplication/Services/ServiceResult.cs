namespace BankingApplication.Services;

/// <summary>
/// Carries a service value or a client-facing error without coupling business logic to HTTP types.
/// Controllers translate invalid results to 400 and conflicts to 409.
/// </summary>
public sealed record ServiceResult<T>(T? Value, string? Error = null, bool IsConflict = false)
{
    /// <summary>Creates a successful result containing a value.</summary>
    public static ServiceResult<T> Success(T value) => new(value);
    /// <summary>Creates a validation failure, translated to HTTP 400 by controllers.</summary>
    public static ServiceResult<T> Invalid(string error) => new(default, error);
    /// <summary>Creates a uniqueness conflict, translated to HTTP 409 by controllers.</summary>
    public static ServiceResult<T> Conflict(string error) => new(default, error, true);
}
