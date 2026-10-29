namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Deletes one email forwarding rule of a domain at the registrar.
/// </summary>
/// <param name="DomainName">Fully-qualified domain name.</param>
/// <param name="Source">Source alias or local part to delete.</param>
public sealed record EmailForwardingRuleRemoval(string DomainName, string Source);
