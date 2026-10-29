using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>What billing needs to know about a domain before it applies a paid renewal to it.</summary>
/// <param name="Id">The domain.</param>
/// <param name="Name">The fully-qualified name.</param>
/// <param name="ExpiresAt">Its current expiry.</param>
/// <param name="Status">Its lifecycle status.</param>
/// <param name="IsAtRegistrar">Whether it is placed at a registrar, so a renewal can be sent there.</param>
public sealed record DomainRenewalStateDto(int Id, string Name, DateTimeOffset ExpiresAt, DomainStatus Status, bool IsAtRegistrar);
