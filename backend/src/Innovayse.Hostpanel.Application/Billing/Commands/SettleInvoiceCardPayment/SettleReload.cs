using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SettleInvoiceCardPayment;

/// <summary>The invoice as the database holds it after a conflicting save.</summary>
/// <param name="Settled">Whether a concurrent request already settled the invoice with this same payment.</param>
/// <param name="Invoice">The freshly loaded invoice; <see langword="null"/> when settled or when it no longer exists.</param>
internal sealed record SettleReload(bool Settled, Invoice? Invoice);
