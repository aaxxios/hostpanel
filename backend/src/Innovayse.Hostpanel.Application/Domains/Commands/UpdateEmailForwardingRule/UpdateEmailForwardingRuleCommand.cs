namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateEmailForwardingRule;

/// <summary>Command to update an existing email forwarding rule on a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain that owns the rule. Nullable so the route id can overlay it: a
/// non-nullable member here would make the body required before <c>with { DomainId = id }</c>
/// ever ran.
/// </param>
/// <param name="RuleId">Primary key of the email forwarding rule to update; overlaid from the route.</param>
/// <param name="Source">New source alias or local part.</param>
/// <param name="Destination">New destination email address.</param>
/// <param name="IsActive">
/// Whether the rule should be active. Defaults to <see langword="true"/> so a caller that omits
/// the field from the body — the admin app's plain edit form never sends it, only its dedicated
/// toggle does — keeps the rule active exactly as the deleted <c>EmailForwardingRuleRequest</c>
/// DTO's own <c>= true</c> property initializer did.
/// </param>
public record UpdateEmailForwardingRuleCommand(
    int? DomainId, int? RuleId, string Source, string Destination, bool IsActive = true);
