namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The switches that can be turned on or off for a domain.</summary>
/// <param name="AutoRenew">Whether the domain is set to auto-renew at expiration.</param>
/// <param name="WhoisPrivacy">Whether WHOIS privacy is enabled.</param>
/// <param name="IsLocked">Whether the domain is locked against unauthorized transfers.</param>
/// <param name="DnsManagement">Whether DNS management is enabled.</param>
/// <param name="EmailForwarding">Whether email forwarding is enabled.</param>
public sealed record DomainFeaturesDto(
    bool AutoRenew,
    bool WhoisPrivacy,
    bool IsLocked,
    bool DnsManagement,
    bool EmailForwarding);
