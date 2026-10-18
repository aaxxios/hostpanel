namespace Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;

/// <summary>Command to renew an existing domain registration for additional years.</summary>
/// <param name="DomainId">
/// Primary key of the domain to renew. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Years">Number of years to extend the registration (1–10).</param>
public record RenewDomainCommand(int? DomainId, int Years);
