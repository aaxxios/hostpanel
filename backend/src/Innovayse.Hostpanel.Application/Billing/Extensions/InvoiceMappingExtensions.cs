using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Maps <see cref="Invoice"/> to <see cref="InvoiceListItemDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class InvoiceMappingExtensions
{
    /// <summary>Projects the query to <see cref="InvoiceListItemDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="InvoiceListItemDto"/>.</returns>
    public static IQueryable<InvoiceListItemDto> SelectDto(this IQueryable<Invoice> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(inv => new InvoiceListItemDto(
            inv.Id, inv.ClientId, string.Empty, inv.Status, inv.DueDate, inv.CreatedAt,
            inv.Total, inv.SubTotal, inv.Tax, inv.InvoiceDate, inv.PaidAt, inv.PaymentMethod, inv.Currency));
    }
}
