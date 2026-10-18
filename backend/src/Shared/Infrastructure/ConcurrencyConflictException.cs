namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Thrown by <c>IUnitOfWork.SaveChangesAsync</c> when an aggregate was modified by another writer
/// between the time a handler read it and the time it tried to save.
/// </summary>
/// <remarks>
/// <para>
/// A framework-level signal, not a business refusal: the Application layer must not reference EF
/// Core, so Infrastructure's <c>IUnitOfWork</c> implementation is the only place allowed to know
/// the underlying cause was an optimistic-concurrency conflict, and it translates the real
/// ORM-specific exception to this persistence-agnostic type. It lives in <c>Shared/</c> rather
/// than in Application for the same reason: it crosses from Infrastructure to the API's own
/// <c>ExceptionHandlingMiddleware</c> (<c>Shared/Middleware/</c>), which answers it 409, and a
/// type only <c>Application/</c> code may define would have nowhere honest to sit on that path.
/// </para>
/// <para>
/// This is why it is not a <c>Result.FailureCode</c> the way every business refusal on this
/// platform is: a handler cannot see the conflict coming to decide on a code for it, because the
/// conflict is only detected inside the very <c>SaveChangesAsync</c> call that already assumed
/// success. Retrying is the caller's own to do -- the row has since changed underneath them, and
/// nothing here decides that for them.
/// </para>
/// </remarks>
public sealed class ConcurrencyConflictException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.</summary>
    /// <param name="message">A human-readable description of the conflict.</param>
    /// <param name="innerException">The original ORM-specific exception, if any.</param>
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
