using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Application.Reports.Extensions;

/// <summary>
/// The one row shape of the Transactions report, shared by the paged view and its CSV export so the
/// two always show the same columns with the same values.
/// </summary>
public static class TransactionReportRowExtensions
{
    /// <summary>Projects ledger rows, already filtered, ordered and cut, to report rows in SQL.</summary>
    /// <param name="transactions">The ledger rows to show.</param>
    /// <param name="clients">The clients the rows are named after.</param>
    /// <returns>The report rows, each in the transaction's own currency and with the invoice it belongs to, not yet materialized.</returns>
    public static IQueryable<TransactionReportRowDto> SelectReportRows(
        this IQueryable<Transaction> transactions, IQueryable<Client> clients)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(clients);

        return transactions
            .Join(clients, t => t.ClientId, c => c.Id,
                (t, c) => new { Transaction = t, ClientName = c.FirstName + " " + c.LastName })
            .Select(x => new TransactionReportRowDto(
                x.Transaction.Id,
                x.Transaction.ClientId,
                x.ClientName,
                x.Transaction.Currency,
                x.Transaction.PaymentMethod,
                x.Transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                x.Transaction.Description,
                x.Transaction.InvoiceId,
                x.Transaction.TransactionId,
                x.Transaction.AmountIn,
                x.Transaction.Fees,
                x.Transaction.AmountOut));
    }
}
