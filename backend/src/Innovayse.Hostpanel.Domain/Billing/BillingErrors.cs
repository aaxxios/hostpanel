namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>
/// Refusal codes the Billing aggregates (<see cref="Currency"/>, <see cref="Invoice"/>,
/// <see cref="Quote"/>) throw as <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so <c>ResourceCoverageTests</c> (which only reflects over
/// <c>HostpanelErrorCodes</c>) still requires a sentence for each one in every language.
/// </remarks>
public static class BillingErrors
{
    /// <summary>The base currency cannot be disabled (400) — <see cref="Currency.Disable"/>.</summary>
    public const string CurrencyBaseDisableNotAllowed = "CURRENCY_BASE_DISABLE_NOT_ALLOWED";

    /// <summary>Only a Draft invoice may be published (400) — <see cref="Invoice.Publish"/>.</summary>
    public const string InvoiceNotDraft = "INVOICE_NOT_DRAFT";

    /// <summary>
    /// The invoice is no longer editable (400) — <see cref="Invoice.AddItem"/> once it has left
    /// Draft/Unpaid.
    /// </summary>
    public const string InvoiceNotEditable = "INVOICE_NOT_EDITABLE";

    /// <summary>
    /// The invoice cannot be paid in its current status (400) — <see cref="Invoice.MarkPaid"/> and
    /// <see cref="Invoice.MarkPaidViaGateway"/>. The same string
    /// <c>Application.Common.Errors.HostpanelErrorCodes.INVOICE_NOT_PAYABLE</c> already answers
    /// with from <c>StartGatewayPaymentHandler</c>'s own pre-check — declared again here, not
    /// renamed, only because Domain may not reference that Application-layer constant.
    /// </summary>
    public const string InvoiceNotPayable = "INVOICE_NOT_PAYABLE";

    /// <summary>
    /// The id names no line item on this invoice (404) — <see cref="Invoice.RemoveItem"/> and
    /// <see cref="Invoice.UpdateItem"/>.
    /// </summary>
    public const string InvoiceItemNotFound = "INVOICE_ITEM_NOT_FOUND";

    /// <summary>
    /// The invoice is not eligible to be marked overdue (400) — <see cref="Invoice.MarkOverdue"/>
    /// requires <c>Unpaid</c>.
    /// </summary>
    public const string InvoiceNotUnpaid = "INVOICE_NOT_UNPAID";

    /// <summary>A paid invoice cannot be cancelled (400) — <see cref="Invoice.Cancel"/>.</summary>
    public const string InvoiceCancelNotAllowed = "INVOICE_CANCEL_NOT_ALLOWED";

    /// <summary>
    /// The invoice must be Paid for this action (400) — <see cref="Invoice.MarkUnpaid"/> and
    /// <see cref="Invoice.Refund"/>.
    /// </summary>
    public const string InvoiceNotPaid = "INVOICE_NOT_PAID";

    /// <summary>
    /// The quote is no longer editable (400) — <see cref="Quote.AddItem"/> once its stage has left
    /// Draft/Delivered/OnHold.
    /// </summary>
    public const string QuoteNotEditable = "QUOTE_NOT_EDITABLE";

    /// <summary>The quote's stage does not allow acceptance (400) — <see cref="Quote.Accept"/>.</summary>
    public const string QuoteNotAcceptable = "QUOTE_NOT_ACCEPTABLE";

    /// <summary>
    /// The id names no line item on this quote (404) — <see cref="Quote.RemoveItem"/> and
    /// <see cref="Quote.UpdateItem"/>.
    /// </summary>
    public const string QuoteItemNotFound = "QUOTE_ITEM_NOT_FOUND";
}
