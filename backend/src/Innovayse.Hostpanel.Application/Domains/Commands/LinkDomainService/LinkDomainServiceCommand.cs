namespace Innovayse.Hostpanel.Application.Domains.Commands.LinkDomainService;

/// <summary>Links a domain to the hosting service it points at, as when both were bought in one order.</summary>
/// <param name="DomainId">The domain to link.</param>
/// <param name="ServiceId">The hosting service the domain belongs to.</param>
public sealed record LinkDomainServiceCommand(int DomainId, int ServiceId);
