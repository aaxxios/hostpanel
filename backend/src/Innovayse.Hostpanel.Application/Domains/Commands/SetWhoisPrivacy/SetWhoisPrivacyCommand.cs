namespace Innovayse.Hostpanel.Application.Domains.Commands.SetWhoisPrivacy;

/// <summary>Command to enable or disable WHOIS privacy for a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to update. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Enabled">The desired WHOIS privacy flag value.</param>
public sealed record SetWhoisPrivacyCommand(int? DomainId, bool Enabled);
