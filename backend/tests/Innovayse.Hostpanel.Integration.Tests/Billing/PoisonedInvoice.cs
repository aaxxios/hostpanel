namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>Names the one invoice whose reminder save <see cref="PoisonedInvoiceSaveInterceptor"/> refuses.</summary>
internal sealed class PoisonedInvoice
{
    /// <summary>The poisoned invoice id.</summary>
    public int InvoiceId { get; set; }
}
