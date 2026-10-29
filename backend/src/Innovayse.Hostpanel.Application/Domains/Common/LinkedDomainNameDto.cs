namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The registered domain a client service is linked to, by the service's id.</summary>
/// <param name="ServiceId">The linked client service's id.</param>
/// <param name="Name">The registered domain name.</param>
public sealed record LinkedDomainNameDto(int ServiceId, string Name);
