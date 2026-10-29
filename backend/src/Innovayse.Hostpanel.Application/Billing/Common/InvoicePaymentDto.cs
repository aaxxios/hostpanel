namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>How an invoice is, or is to be, paid.</summary>
/// <param name="Method">Preferred payment method; null when not specified.</param>
/// <param name="GatewayTransactionId">Payment gateway reference; null when unpaid.</param>
public sealed record InvoicePaymentDto(string? Method, string? GatewayTransactionId);
