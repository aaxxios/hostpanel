namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>
/// The codes the shared edge produces on its own, before or around any handler: the house codes
/// <c>ExceptionHandlingMiddleware</c> answers with, and the fallback for a codeless handler failure.
/// Every other code belongs to the Application layer's registry.
/// </summary>
public static class SharedErrorCodes
{
    /// <summary>A command or query failed FluentValidation on the bus: answered 400.</summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>An unmapped exception: answered 500; the caller learns nothing about it.</summary>
    public const string InternalError = "INTERNAL_ERROR";

    /// <summary>An <see cref="UnauthorizedAccessException"/> reached the edge: answered 401.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>
    /// An authenticated caller failed an authorization policy or the bearer scheme's own forbid: answered 403.
    /// </summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>
    /// A state-changing request rode on the session cookie alone without the header a cross-site page
    /// cannot set: answered 403.
    /// </summary>
    public const string CsrfHeaderMissing = "CSRF_HEADER_MISSING";

    /// <summary>A <see cref="KeyNotFoundException"/> reached the edge: answered 404.</summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>An <see cref="InvalidOperationException"/> reached the edge: answered 400.</summary>
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";

    /// <summary>
    /// A <c>ConcurrencyConflictException</c> (<c>Shared/Infrastructure/</c>) reached the edge: answered
    /// 409. Raised by <c>IHostpanelDbContext.SaveChangesAsync</c> when another writer modified the same
    /// aggregate between the read and the save; the caller's own to retry.
    /// </summary>
    public const string Conflict = "CONFLICT";

    /// <summary>The request body could not be read or parsed: answered 400.</summary>
    public const string InvalidRequest = "INVALID_REQUEST";

    /// <summary>
    /// A restricted command was refused because the deployment is a public demo: answered 403
    /// by an exact <c>StatusMapping</c> row (the name carries none of the status tokens).
    /// </summary>
    public const string DemoActionDisabled = "DEMO_ACTION_DISABLED";

    /// <summary>A handler failed without a code; <c>BaseApiController.MapFailure</c> answers with this one.</summary>
    public const string Unexpected = "HOSTPANEL_UNEXPECTED";

    /// <summary>The edge rate limiter refused the request (429, with <c>Retry-After</c>), via the <c>RATE_LIMIT</c> token.</summary>
    public const string RateLimited = "HOSTPANEL_RATE_LIMITED";
}
