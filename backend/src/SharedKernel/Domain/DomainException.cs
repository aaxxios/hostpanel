namespace Innovayse.Hostpanel.SharedKernel.Domain;

/// <summary>
/// An invariant was violated inside the domain.
/// </summary>
/// <remarks>
/// The last line of defence, not the API contract: a handler that can foresee the refusal asks
/// the aggregate's predicate first and returns a <c>Result.Failure</c>. When one of these reaches
/// the edge anyway, the exception middleware turns it into a 409, so a bug surfaces as a refusal
/// rather than as a corrupted row.
/// </remarks>
public abstract class DomainException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="DomainException"/> class.</summary>
    /// <param name="code">The error code the edge maps to a status, SCREAMING_SNAKE.</param>
    /// <param name="message">What was violated, for the log.</param>
    protected DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>The error code the edge maps to a status.</summary>
    public string Code { get; }
}
