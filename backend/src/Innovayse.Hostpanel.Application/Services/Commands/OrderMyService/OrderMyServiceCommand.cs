namespace Innovayse.Hostpanel.Application.Services.Commands.OrderMyService;

/// <summary>
/// Command for a client ordering a service for their own account. Names no account and no amount:
/// the handler resolves the caller and the price is read from the product.
/// </summary>
/// <param name="ProductId">The product being ordered.</param>
/// <param name="BillingCycle">Billing cycle: monthly or annual.</param>
/// <param name="PaymentMethod">Payment method used, when the client chose one.</param>
/// <param name="Domain">Domain name to pre-fill, when purchased alongside hosting.</param>
public sealed record OrderMyServiceCommand(int ProductId, string BillingCycle, string? PaymentMethod = null, string? Domain = null);
