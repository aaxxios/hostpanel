namespace Innovayse.Hostpanel.Application.Billing.Common.Enums;

/// <summary>An action applied to every selected invoice at once. Travels on the wire by name.</summary>
public enum InvoiceBulkAction
{
    /// <summary>Settles each payable invoice as paid manually.</summary>
    MarkPaid = 0,

    /// <summary>Reverses each paid invoice back to Unpaid.</summary>
    MarkUnpaid = 1,

    /// <summary>Cancels each invoice that may still be cancelled, returning its applied credit.</summary>
    MarkCancelled = 2,

    /// <summary>Creates a Draft copy of each invoice.</summary>
    Duplicate = 3,

    /// <summary>Deletes each invoice that may be deleted (Draft or Cancelled).</summary>
    Delete = 4,
}
