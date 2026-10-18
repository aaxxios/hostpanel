namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateNameservers;

/// <summary>Command to replace the nameserver list for a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to update. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Nameservers">New ordered list of nameserver hostnames (minimum 2).</param>
public record UpdateNameserversCommand(int? DomainId, IReadOnlyList<string> Nameservers);
