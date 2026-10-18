namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The invoice statuses the source system sends for a source invoice record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceInvoiceStatuses
{
    /// <summary>The source value <c>Draft</c>.</summary>
    public const string Draft = "Draft";

    /// <summary>The source value <c>Unpaid</c>.</summary>
    public const string Unpaid = "Unpaid";

    /// <summary>The source value <c>Paid</c>.</summary>
    public const string Paid = "Paid";

    /// <summary>The source value <c>Overdue</c>.</summary>
    public const string Overdue = "Overdue";

    /// <summary>The source value <c>Cancelled</c>.</summary>
    public const string Cancelled = "Cancelled";

    /// <summary>The source value <c>Refunded</c>.</summary>
    public const string Refunded = "Refunded";

    /// <summary>The source value <c>Collections</c>.</summary>
    public const string Collections = "Collections";

    /// <summary>The source value <c>PaymentPending</c>.</summary>
    public const string PaymentPending = "PaymentPending";
}
