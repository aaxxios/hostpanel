namespace Innovayse.Hostpanel.Application.Domains.Commands.RenewOrderedDomain;

/// <summary>Renews a domain named by a paid order line, for the client who placed the order.</summary>
/// <param name="ClientId">The client who paid for the renewal; the domain must be theirs.</param>
/// <param name="DomainName">Fully-qualified name from the order line.</param>
/// <param name="Years">Period bought, in years.</param>
public sealed record RenewOrderedDomainCommand(int ClientId, string DomainName, int Years);
