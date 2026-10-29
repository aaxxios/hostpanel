namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Switches a boolean setting (auto-renew, WHOIS privacy or registrar lock) of a domain at the registrar.
/// </summary>
/// <param name="Domain">The domain to change.</param>
/// <param name="Enabled">Whether the setting should be on.</param>
public sealed record RegistrarFlagChange(DomainRef Domain, bool Enabled);
