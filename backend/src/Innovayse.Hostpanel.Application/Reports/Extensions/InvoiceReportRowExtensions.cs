using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Application.Reports.Extensions;

/// <summary>
/// The one row shape of the Invoices report, shared by the paged view and its CSV export so the two
/// always show the same columns with the same values.
/// </summary>
public static class InvoiceReportRowExtensions
{
    /// <summary>Projects invoices, already filtered, ordered and cut, to report rows in SQL.</summary>
    /// <param name="invoices">The invoices to show.</param>
    /// <param name="clients">The clients the rows are named after.</param>
    /// <returns>The report rows, amounts in each invoice's own currency, not yet materialized.</returns>
    public static IQueryable<InvoiceReportRowDto> SelectReportRows(
        this IQueryable<Invoice> invoices, IQueryable<Client> clients)
    {
        ArgumentNullException.ThrowIfNull(invoices);
        ArgumentNullException.ThrowIfNull(clients);

        return invoices
            .Join(clients, i => i.ClientId, c => c.Id,
                (i, c) => new { Invoice = i, ClientName = c.FirstName + " " + c.LastName })
            .Select(x => new InvoiceReportRowDto(
                x.Invoice.Id,
                x.Invoice.ClientId,
                x.ClientName,
                new InvoiceReportDatesDto(
                    x.Invoice.CreatedAt.ToString("yyyy-MM-dd"),
                    x.Invoice.DueDate.ToString("yyyy-MM-dd"),
                    x.Invoice.PaidAt != null ? x.Invoice.PaidAt.Value.ToString("yyyy-MM-dd") : null,
                    x.Invoice.Transactions.Any(t => t.Type == InvoiceTransactionType.Refund)
                        ? x.Invoice.Transactions.Where(t => t.Type == InvoiceTransactionType.Refund).Max(t => t.Date).ToString("yyyy-MM-dd")
                        : null),
                x.Invoice.Currency,
                new InvoiceAmountsDto(x.Invoice.SubTotal, x.Invoice.Credit, x.Invoice.Tax, x.Invoice.TaxRate, x.Invoice.Total),
                x.Invoice.Status,
                x.Invoice.PaymentMethod,
                x.Invoice.Notes));
    }
}
