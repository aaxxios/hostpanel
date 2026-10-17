namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>
/// Input for <c>IPaymentGateway.ChargeAsync</c> (<c>Innovayse.Hostpanel.Application.Common.Interfaces</c>
/// -- Domain cannot reference it, so this cannot be a resolvable <c>cref</c>).
/// </summary>
/// <param name="ClientId">The client being charged.</param>
/// <param name="InvoiceId">The invoice being paid.</param>
/// <param name="Amount">Charge amount.</param>
/// <param name="Currency">ISO 4217 currency code (e.g. "USD").</param>
public record ChargeInstruction(int ClientId, int InvoiceId, decimal Amount, string Currency);
