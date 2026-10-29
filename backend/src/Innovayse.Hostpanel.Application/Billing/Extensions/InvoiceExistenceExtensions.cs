using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Answers whether the invoice an id taken from input points at exists. A Billing handler that stores
/// such an id checks it here before saving, so a missing invoice is refused with its
/// <c>INVOICE_NOT_FOUND</c> code instead of surfacing as a failed save.
/// </summary>
public static class InvoiceExistenceExtensions
{
    /// <summary>Whether an invoice with this id exists.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="invoiceId">The id to look for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when the invoice exists.</returns>
    public static Task<bool> InvoiceExistsAsync(this IHostpanelDbContext context, int invoiceId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Invoices.AsNoTracking().AnyAsync(x => x.Id == invoiceId, ct);
    }
}
