namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>
/// Result pattern for operation outcomes. Avoids exceptions for expected failure cases.
/// Supports optional field-level errors for business rule violations that should be displayed
/// on specific form fields.
/// </summary>
public class Result
{
    /// <summary>Initializes a new result.</summary>
    /// <param name="isSuccess">Whether operation succeeded.</param>
    /// <param name="error">Error message if failed.</param>
    /// <param name="errorCode">Error code for client handling.</param>
    /// <param name="errors">Optional field-level errors for form binding.</param>
    protected Result(bool isSuccess, string? error, string? errorCode = null,
        Dictionary<string, List<string>>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        Errors = errors;
    }

    /// <summary>Whether operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Whether operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Error message if failed.</summary>
    public string? Error { get; }

    /// <summary>Error code for client handling (e.g. NOT_FOUND, VALIDATION_ERROR).</summary>
    public string? ErrorCode { get; }

    /// <summary>Field-level errors for business rule violations (field name → messages).</summary>
    public Dictionary<string, List<string>>? Errors { get; }

    /// <summary>Create successful result.</summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Create a refusal carrying only its code. The sentence a person reads is resolved from
    /// <c>ErrorMessages.resx</c> by that code at the API boundary, so a handler has none to supply
    /// (rules/localization.md).
    /// </summary>
    /// <remarks>
    /// A separate name rather than a one-argument <c>Failure</c> overload: <c>Failure(string)</c>
    /// already binds its single argument to the <b>message</b>, leaving the code null, and that call
    /// fails silently — the boundary answers <c>HOSTPANEL_UNEXPECTED</c> with the code sitting in the
    /// message slot.
    /// </remarks>
    /// <param name="errorCode">A <c>HostpanelErrorCodes</c> constant.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result FailureCode(string errorCode) => new(false, null, errorCode);

    /// <summary>Create failure result with error.</summary>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string error, string? errorCode = null) =>
        new(false, error, errorCode);

    /// <summary>Create failure result bound to a specific form field.</summary>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="fieldName">Form field name this error is bound to (PascalCase).</param>
    /// <returns>A failed <see cref="Result"/> with field-level errors.</returns>
    public static Result Failure(string error, string errorCode, string fieldName) =>
        new(false, error, errorCode, new Dictionary<string, List<string>> { { fieldName, [error] } });

    /// <summary>
    /// Create failure result carrying one structured detail the caller can act on — which attachment failed —
    /// alongside the sentence.
    /// </summary>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="detailKey">The camelCase key the detail is read under, e.g. <c>attachmentId</c>.</param>
    /// <param name="detailValue">The detail itself — an identifier, never a sentence.</param>
    /// <returns>A failed <see cref="Result"/> whose <c>Errors</c> holds the one detail.</returns>
    public static Result FailureWithDetail(string error, string errorCode, string detailKey, string detailValue) =>
        new(false, error, errorCode, new Dictionary<string, List<string>> { { detailKey, [detailValue] } });

    /// <summary>Create successful result with value.</summary>
    /// <typeparam name="T">Result value type.</typeparam>
    /// <param name="value">Result value.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>Create failure result with error.</summary>
    /// <typeparam name="T">Result value type.</typeparam>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(string error, string? errorCode = null) =>
        Result<T>.Failure(error, errorCode);

    /// <summary>Create failure result bound to a specific form field.</summary>
    /// <typeparam name="T">Result value type.</typeparam>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="fieldName">Form field name this error is bound to (PascalCase).</param>
    /// <returns>A failed <see cref="Result{T}"/> with field-level errors.</returns>
    public static Result<T> Failure<T>(string error, string errorCode, string fieldName) =>
        Result<T>.Failure(error, errorCode, fieldName);

    /// <summary>Create failure result with a value type and one structured detail.</summary>
    /// <typeparam name="T">Result value type.</typeparam>
    /// <param name="error">Human-readable error message.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="detailKey">The camelCase key the detail is read under.</param>
    /// <param name="detailValue">The detail itself.</param>
    /// <returns>A failed <see cref="Result{T}"/> whose <c>Errors</c> holds the one detail.</returns>
    public static Result<T> FailureWithDetail<T>(
        string error, string errorCode, string detailKey, string detailValue) =>
        Result<T>.FailureWithDetail(error, errorCode, detailKey, detailValue);
}
