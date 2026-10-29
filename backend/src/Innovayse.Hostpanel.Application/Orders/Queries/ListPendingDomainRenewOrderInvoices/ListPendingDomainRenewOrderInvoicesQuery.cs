namespace Innovayse.Hostpanel.Application.Orders.Queries.ListPendingDomainRenewOrderInvoices;

/// <summary>
/// Lists the invoices of a client's still pending orders that carry a renew line for one domain name,
/// so recurring billing can tell whether a renew order is already billing that domain.
/// </summary>
/// <param name="ClientId">The client.</param>
/// <param name="DomainName">The domain name, as an order line carries it.</param>
public sealed record ListPendingDomainRenewOrderInvoicesQuery(int ClientId, string DomainName);
