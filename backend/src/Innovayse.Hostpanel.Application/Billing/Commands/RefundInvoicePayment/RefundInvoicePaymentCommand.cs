using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment;

/// <summary>Command to refund all or part of what a paid invoice took.</summary>
/// <param name="InvoiceId">The paid invoice to refund.</param>
/// <param name="Amount">Amount to refund, in the invoice currency; greater than 0 and at most what is still refundable.</param>
/// <param name="RefundType">How the refund is processed.</param>
/// <param name="Gateway">Payment method recorded for a Manual refund; ignored for the other types, which know their own.</param>
/// <param name="RefundTransactionId">External refund reference; required for a Manual refund.</param>
/// <param name="Notes">Optional refund notes.</param>
public sealed record RefundInvoicePaymentCommand(
    int InvoiceId,
    decimal Amount,
    RefundType RefundType,
    string? Gateway = null,
    string? RefundTransactionId = null,
    string? Notes = null);
