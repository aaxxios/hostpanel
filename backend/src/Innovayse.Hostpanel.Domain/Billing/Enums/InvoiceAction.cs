namespace Innovayse.Hostpanel.Domain.Billing.Enums;
/// <summary>Determines how and when a billable item should be invoiced.</summary>
public enum InvoiceAction
{
    /// <summary>Item stays uninvoiced until manually selected.</summary>
    DontInvoice = 0,

    /// <summary>Item will be invoiced on the next scheduled cron run.</summary>
    InvoiceOnNextCron = 1,

    /// <summary>Item will be added to the client's next invoice.</summary>
    AddToNextInvoice = 2,

    /// <summary>Item will be invoiced when its due date arrives.</summary>
    InvoiceForDueDate = 3,

    /// <summary>Item recurs at a set interval and is automatically invoiced each cycle.</summary>
    Recur = 4,
}
