using Innovayse.Hostpanel.Application.Clients.Common;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyCardGatewayCustomer;

/// <summary>
/// Requests the <see cref="CardGatewayCustomerDto"/> of the caller's own client record, so the
/// Billing module can reach the saved payment methods without reading the Clients tables.
/// </summary>
/// <remarks>Carries no user id: whose record is resolved inside the handler from the credential.</remarks>
public sealed record GetMyCardGatewayCustomerQuery();
