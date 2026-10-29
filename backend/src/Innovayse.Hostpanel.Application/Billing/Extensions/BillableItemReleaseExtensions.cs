using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Puts billable items back to "not invoiced" when the invoice they were put on is deleted, so the
/// next invoicing run bills them. The one place that rule lives, shared by every way an invoice is deleted.
/// </summary>
public static class BillableItemReleaseExtensions
{
    /// <summary>
    /// Stages the release of every billable item that sits on one of the invoices; the caller saves, in
    /// the same unit of work that removes the invoices.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="invoiceIds">The invoices about to be deleted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the items are loaded and released.</returns>
    public static async Task ReleaseBillableItemsAsync(
        this IHostpanelDbContext context, IReadOnlyCollection<int> invoiceIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(invoiceIds);

        if (invoiceIds.Count == 0)
        {
            return;
        }

        var items = await context.BillableItems
            .Where(x => x.InvoiceId != null && invoiceIds.Contains(x.InvoiceId.Value))
            .ToListAsync(ct);
        foreach (var item in items)
        {
            item.ReleaseFromInvoice();
        }
    }
}
