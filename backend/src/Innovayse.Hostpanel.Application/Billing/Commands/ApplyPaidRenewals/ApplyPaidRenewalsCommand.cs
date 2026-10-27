namespace Innovayse.Hostpanel.Application.Billing.Commands.ApplyPaidRenewals;

/// <summary>
/// Applies every not-yet-applied renewal line of one paid invoice. Idempotent: the line's
/// <c>RenewalAppliedAt</c> marker, not the payment event, decides whether anything happens, so the
/// payment handler and the daily sweep may both send it.
/// </summary>
/// <param name="InvoiceId">The invoice.</param>
public record ApplyPaidRenewalsCommand(int InvoiceId);
