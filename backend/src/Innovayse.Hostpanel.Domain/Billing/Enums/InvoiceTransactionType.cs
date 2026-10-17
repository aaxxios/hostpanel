namespace Innovayse.Hostpanel.Domain.Billing.Enums;
/// <summary>Classifies financial transactions recorded against an <see cref="Invoice"/>.</summary>
public enum InvoiceTransactionType
{
    /// <summary>A payment made towards the invoice balance.</summary>
    Payment = 0,

    /// <summary>A refund returned to the client.</summary>
    Refund = 1,

    /// <summary>A credit applied to reduce the invoice balance.</summary>
    Credit = 2,
}
