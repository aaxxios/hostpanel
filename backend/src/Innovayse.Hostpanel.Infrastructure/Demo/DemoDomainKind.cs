using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>The state and distance to expiry of a domain handed out to a demo customer.</summary>
/// <param name="Status">The domain state.</param>
/// <param name="ExpiresInDays">Days from now to expiry; negative when already past.</param>
internal sealed record DemoDomainKind(DomainStatus Status, int ExpiresInDays);
