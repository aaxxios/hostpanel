using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// The one definition of an invoice that is still open for renewal purposes, shared by the renewal
/// run (which must not raise a second line for a target already billed) and by a client renew order
/// (which must take the domain off such an invoice first), so the two can never disagree.
/// </summary>
public static class RenewalInvoiceStatuses
{
    /// <summary>Statuses of an invoice that is still open: not paid, not cancelled, not refunded.</summary>
    public static readonly InvoiceStatus[] Open =
        [InvoiceStatus.Draft, InvoiceStatus.Unpaid, InvoiceStatus.Overdue, InvoiceStatus.Collections, InvoiceStatus.PaymentPending];
}
