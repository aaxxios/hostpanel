namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Where one refund went, the reference it carries and how much it returned, as recorded on the invoice and the ledger.</summary>
/// <param name="Gateway">Payment method or gateway name the refund went through.</param>
/// <param name="Reference">Reference of the refund.</param>
/// <param name="Amount">Amount this refund returned, in the invoice currency.</param>
public sealed record RefundOutcome(string Gateway, string Reference, decimal Amount);
