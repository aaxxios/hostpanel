namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The language, billing currency and payment preferences of a client, with the staff-only notes.</summary>
/// <param name="Language">
/// The account holder's chosen UI language, or null where this deployment does not own the
/// account and therefore holds no language for it.
/// </param>
/// <param name="Currency">ISO 4217 currency code, when one is set.</param>
/// <param name="PaymentMethod">Preferred payment method label.</param>
/// <param name="BillingContact">Billing contact reference.</param>
/// <param name="AdminNotes">Internal admin notes.</param>
public sealed record ClientPreferencesDto(
    string? Language,
    string? Currency,
    string? PaymentMethod,
    string? BillingContact,
    string? AdminNotes);
