namespace Innovayse.Hostpanel.Application.Domains.Commands.SetAutoRenew;

/// <summary>Command to enable or disable automatic renewal for a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to update. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Value">The desired auto-renew flag value.</param>
public sealed record SetAutoRenewCommand(int? DomainId, bool Value);
