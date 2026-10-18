using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain;

/// <summary>
/// The default concrete <see cref="DomainException"/> for a refusal inside an aggregate.
/// </summary>
/// <remarks>
/// hostpanel's conformance plan (A6 ruling 4) keeps <see cref="DomainException"/> itself abstract
/// and asks for "per-feature subtypes only where a caller distinguishes them" — no caller in this
/// codebase catches a specific <see cref="DomainException"/> subtype by name today, so one shared
/// concrete type carries every aggregate's refusal instead of one class per rule. A feature that
/// later needs a caller-visible subtype can still add one; this type does not preclude it.
/// </remarks>
/// <param name="code">The error code the edge maps to a status, SCREAMING_SNAKE.</param>
/// <param name="message">What was violated, for the log.</param>
public sealed class DomainRuleException(string code, string message) : DomainException(code, message)
{
}
