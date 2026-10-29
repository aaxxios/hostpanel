using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// The few facts about an invoice that another module shows or acts on (the status beside an
/// order, the amount and currency a card payment is opened for), so it never reads the Billing
/// tables itself.
/// </summary>
/// <param name="Id">The invoice id.</param>
/// <param name="ClientId">The invoiced client.</param>
/// <param name="Status">The invoice's current status.</param>
/// <param name="Total">The invoice total, in <paramref name="Currency"/>.</param>
/// <param name="AmountDue">What is still owed: the total less the payments already recorded, in <paramref name="Currency"/>.</param>
/// <param name="Currency">ISO 4217 alpha code the invoice is billed in.</param>
/// <param name="IsPaidInFull">
/// Whether the invoice is Paid and the money covers it, counting an invoice settled without a payment
/// row (credit already booked, imported history) as covered, which <paramref name="AmountDue"/> alone does not.
/// </param>
public sealed record InvoiceBriefDto(
    int Id, int ClientId, InvoiceStatus Status, decimal Total, decimal AmountDue, string Currency, bool IsPaidInFull);
