namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// The card gateway customer a client record is linked to, for the Billing module's saved payment
/// method screens and charges.
/// </summary>
/// <param name="ClientId">The client record's id.</param>
/// <param name="CustomerId">
/// The card gateway customer id, or <see langword="null"/> when the client has never saved a payment method.
/// </param>
public sealed record CardGatewayCustomerDto(int ClientId, string? CustomerId);
