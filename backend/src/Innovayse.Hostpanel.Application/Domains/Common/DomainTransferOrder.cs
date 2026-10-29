using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Parameters required to initiate an incoming domain transfer via a registrar provider.
/// </summary>
/// <param name="DomainName">Fully-qualified domain name to transfer (e.g. "example.com").</param>
/// <param name="EppCode">Authorization/EPP code obtained from the losing registrar.</param>
/// <param name="WhoisPrivacy">Whether to enable WHOIS privacy after the transfer completes.</param>
/// <param name="Contact">The registrant contact, built from the client's details, sent for every contact role.</param>
public record DomainTransferOrder(string DomainName, string EppCode, bool WhoisPrivacy, DomainContact Contact);
