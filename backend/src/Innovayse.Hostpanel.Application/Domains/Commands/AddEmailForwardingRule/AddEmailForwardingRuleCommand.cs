namespace Innovayse.Hostpanel.Application.Domains.Commands.AddEmailForwardingRule;

/// <summary>Command to add an email forwarding rule to a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to add the rule to. Nullable so the route id can overlay it: a
/// non-nullable member here would make the body required before <c>with { DomainId = id }</c>
/// ever ran.
/// </param>
/// <param name="Source">Source alias or local part (e.g. "info").</param>
/// <param name="Destination">Destination email address.</param>
public record AddEmailForwardingRuleCommand(int? DomainId, string Source, string Destination);
