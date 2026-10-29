using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>One of the demo client's own domains.</summary>
/// <param name="Extension">The suffix appended to the client's name.</param>
/// <param name="Status">The domain state.</param>
/// <param name="ExpiresInDays">Days from now to expiry; negative when already past.</param>
internal sealed record DemoClientDomain(string Extension, DomainStatus Status, int ExpiresInDays);
