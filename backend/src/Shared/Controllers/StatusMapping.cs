using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.Shared.Controllers;

/// <summary>
/// The one place an error code becomes an HTTP status and a title. Exact codes first, then
/// tokens, first match wins — the table in <c>rules/controllers.md</c>, in that order.
/// </summary>
/// <remarks>
/// The generic arms (<c>RATE_LIMIT</c>, <c>_LOCKED</c>, <c>_UPSTREAM_FAILED</c>, hostpanel's
/// <c>_SEND_FAILED</c>/<c>_TOO_LARGE</c>/<c>_TOKEN_INVALID</c>, <c>_NOT_CONFIGURED</c>)
/// sit after every specific row, so <c>SHARE_NOT_FOUND_UPSTREAM_FAILED</c> is still 404 and
/// <c>VALIDATION_NOT_CONFIGURED</c> still 422. A code that merely contains <c>RATE_LIMIT</c> answers
/// 429 — name new codes so they do not contain that token unless that status is meant.
/// </remarks>
public static class StatusMapping
{
    /// <summary>Maps an error code to the status and title the response carries.</summary>
    /// <param name="errorCode">The handler's code, SCREAMING_SNAKE.</param>
    /// <returns>The status and the title; 400 "Bad Request" for a code no row names.</returns>
    public static (int Status, string Title) For(string errorCode)
    {
        return errorCode switch
        {
            SharedErrorCodes.InvalidRequest => (400, "Bad Request"),
            // Reserved for ExceptionHandlingMiddleware (FluentValidation → 400). Exact row above the
            // VALIDATION token so a stray handler use still answers 400; handler codes use
            // *_VALIDATION_FAILED → 422.
            SharedErrorCodes.ValidationError => (400, "Validation Failed"),
            "QUOTA_EXCEEDED" or "LIMIT_EXCEEDED" => (402, "Payment Required"),
            "PAYLOAD_TOO_LARGE" => (413, "Content Too Large"),
            "CONTENT_REJECTED" => (422, "Unprocessable Content"),
            "CONFIRMATION_REQUIRED" => (409, "Conflict"),
            "CONCURRENCY_LIMITED" => (429, "Too Many Requests"),
            "UPLOAD_FAILED" => (502, "Bad Gateway"),
            // Exact row: the code names a policy, not a missing grant, so no FORBIDDEN token is in it.
            SharedErrorCodes.DemoActionDisabled => (403, "Forbidden"),
            // Capacity, not configuration: naming it *_NOT_CONFIGURED would answer correctly by
            // accident and mislabel a full fleet as an unfinished deployment.
            "NO_ELIGIBLE_SERVER" => (503, "Service Unavailable"),
            // Exact row: the user is not on the caller's account, which reads as absent (404), not as a
            // malformed request; the name carries no NOT_FOUND token for the generic arm to catch.
            "CLIENT_USER_NOT_LINKED" => (404, "Not Found"),
            // Exact row: the name says "permission", not one of the FORBIDDEN / ACCESS_DENIED tokens the
            // generic arms below catch, and a missing grant is a refusal (403), never a malformed request.
            "CLIENT_PERMISSION_DENIED" => (403, "Forbidden"),
            var code when code.Contains("UNAUTHORIZED", StringComparison.Ordinal) => (401, "Unauthorized"),
            var code when code.Contains("FORBIDDEN", StringComparison.Ordinal) => (403, "Forbidden"),
            var code when code.Contains("ACCESS_DENIED", StringComparison.Ordinal) => (403, "Forbidden"),
            var code when code.Contains("NOT_FOUND", StringComparison.Ordinal) => (404, "Not Found"),
            var code when code.Contains("CONFLICT", StringComparison.Ordinal) => (409, "Conflict"),
            var code when code.Contains("ALREADY", StringComparison.Ordinal) => (409, "Conflict"),
            var code when code.EndsWith("_EXISTS", StringComparison.Ordinal) => (409, "Conflict"),
            var code when code.Contains("VALIDATION", StringComparison.Ordinal) => (422, "Validation Failed"),
            var code when code.Contains("RATE_LIMIT", StringComparison.Ordinal) => (429, "Too Many Requests"),
            var code when code.EndsWith("_LOCKED", StringComparison.Ordinal) => (423, "Locked"),
            var code when code.EndsWith("_UPSTREAM_FAILED", StringComparison.Ordinal) => (502, "Bad Gateway"),
            // hostpanel's three suffix rows: the codes predate this table and the frontends branch
            // on them, so the table learns their statuses instead of the codes being renamed.
            var code when code.EndsWith("_SEND_FAILED", StringComparison.Ordinal) => (502, "Bad Gateway"),
            var code when code.EndsWith("_TOO_LARGE", StringComparison.Ordinal) => (413, "Content Too Large"),
            var code when code.EndsWith("_TOKEN_INVALID", StringComparison.Ordinal) => (403, "Forbidden"),
            var code when code.Contains("UNEXPECTED", StringComparison.Ordinal) => (500, "Unexpected Error"),
            var code when code.Contains("SERVICE_UNAVAILABLE", StringComparison.Ordinal)
                => (503, "Service Unavailable"),
            var code when code.Contains("TIMEOUT", StringComparison.Ordinal) => (503, "Service Unavailable"),
            var code when code.EndsWith("_NOT_CONFIGURED", StringComparison.Ordinal)
                => (503, "Service Unavailable"),
            _ => (400, "Bad Request"),
        };
    }
}
