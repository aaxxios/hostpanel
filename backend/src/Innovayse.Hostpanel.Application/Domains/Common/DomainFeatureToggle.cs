namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Switches a name-addressed feature (email forwarding or DNS management) of a domain at the registrar.
/// </summary>
/// <param name="DomainName">Fully-qualified domain name.</param>
/// <param name="Enabled">Whether the feature should be on.</param>
public sealed record DomainFeatureToggle(string DomainName, bool Enabled);
