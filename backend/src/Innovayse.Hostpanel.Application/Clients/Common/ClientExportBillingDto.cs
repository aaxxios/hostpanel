namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>How the account is billed, as the <c>profileData</c> section of a client data export lists it.</summary>
/// <param name="Currency">Billing currency code, when one is set.</param>
/// <param name="CreditBalance">Credit held on the account, in the billing currency.</param>
/// <param name="PaymentMethod">Payment method the account prefers, when one is set.</param>
/// <param name="BillingContact">Name or note naming who receives billing correspondence.</param>
public sealed record ClientExportBillingDto(
    string? Currency,
    decimal CreditBalance,
    string? PaymentMethod,
    string? BillingContact);
