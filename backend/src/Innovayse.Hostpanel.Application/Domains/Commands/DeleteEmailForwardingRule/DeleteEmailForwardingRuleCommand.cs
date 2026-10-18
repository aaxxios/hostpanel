namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteEmailForwardingRule;

/// <summary>Command to delete an email forwarding rule from a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain that owns the rule. Nullable so the route id can overlay it: a
/// non-nullable member here would make the body required before <c>with { DomainId = id }</c>
/// ever ran.
/// </param>
/// <param name="RuleId">Primary key of the email forwarding rule to delete; overlaid from the route.</param>
public record DeleteEmailForwardingRuleCommand(int? DomainId, int? RuleId);
