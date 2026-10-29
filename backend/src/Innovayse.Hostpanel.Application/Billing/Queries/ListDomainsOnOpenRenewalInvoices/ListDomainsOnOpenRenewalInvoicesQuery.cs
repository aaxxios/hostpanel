namespace Innovayse.Hostpanel.Application.Billing.Queries.ListDomainsOnOpenRenewalInvoices;

/// <summary>
/// Asks which of the given domains have a renewal line on an open invoice, so the Domains module can
/// tell which clients are already being reminded by a renewal invoice without reading invoices itself.
/// </summary>
/// <param name="RegisteredDomainIds">The domains to check.</param>
public sealed record ListDomainsOnOpenRenewalInvoicesQuery(IReadOnlyList<int> RegisteredDomainIds);
