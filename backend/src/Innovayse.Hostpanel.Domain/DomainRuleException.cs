using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain;

/// <summary>
/// The default concrete <see cref="DomainException"/> for a refusal inside an aggregate.
/// </summary>
/// <remarks>
/// <see cref="DomainException"/> stays abstract, and a per-feature subtype exists only where a
/// caller distinguishes it. No caller catches a specific <see cref="DomainException"/> subtype by
/// name, so one shared concrete type carries every aggregate's refusal instead of one class per
/// rule. A feature that needs a caller-visible subtype can still add one.
/// </remarks>
/// <param name="code">The error code the edge maps to a status, SCREAMING_SNAKE.</param>
/// <param name="message">What was violated, for the log.</param>
public sealed class DomainRuleException(string code, string message) : DomainException(code, message)
{
}
