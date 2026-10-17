using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Billing.Commands.BulkInvoiceAction;

/// <summary>Performs a bulk action on multiple invoices and returns the count of affected invoices.</summary>
public sealed class BulkInvoiceActionHandler
{
    /// <summary>Supported bulk action: mark invoices as paid.</summary>
    private const string ActionMarkPaid = "MarkPaid";

    /// <summary>Supported bulk action: mark invoices as unpaid.</summary>
    private const string ActionMarkUnpaid = "MarkUnpaid";

    /// <summary>Supported bulk action: cancel invoices.</summary>
    private const string ActionMarkCancelled = "MarkCancelled";

    /// <summary>Supported bulk action: duplicate invoices.</summary>
    private const string ActionDuplicate = "Duplicate";

    /// <summary>Supported bulk action: delete invoices.</summary>
    private const string ActionDelete = "Delete";

    /// <summary>Ledger payment method recorded for invoices an admin marks paid in bulk.</summary>
    private const string BulkPaymentMethod = PaymentMethodNames.Manual;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public BulkInvoiceActionHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles <see cref="BulkInvoiceActionCommand"/>.</summary>
    /// <param name="cmd">The bulk action command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the number of invoices affected, or <c>BULK_INVOICE_ACTION_UNKNOWN</c> when
    /// <paramref name="cmd"/> names an action this endpoint does not recognise.
    /// </returns>
    public async Task<Result<int>> HandleAsync(BulkInvoiceActionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .Where(i => cmd.InvoiceIds.Contains(i.Id))
            .ToListAsync(ct);
        var affected = 0;

        foreach (var invoice in invoices)
        {
            switch (cmd.Action)
            {
                case ActionMarkPaid:
                    if (invoice.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue)
                    {
                        var txId = $"bulk-{invoice.Id}-{DateTimeOffset.UtcNow.Ticks}";
                        invoice.MarkPaid(txId);

                        // Same save as the Paid mark: one ledger row per invoice settled.
                        if (invoice.Total > 0)
                        {
                            _context.Transactions.Add(Transaction.CreateInvoicePayment(
                                invoice, DateTimeOffset.UtcNow, BulkPaymentMethod, txId, invoice.Total, 0m));
                        }

                        affected++;
                    }

                    break;

                case ActionMarkUnpaid:
                    if (invoice.Status == InvoiceStatus.Paid)
                    {
                        invoice.MarkUnpaid();
                        affected++;
                    }

                    break;

                case ActionMarkCancelled:
                    if (invoice.Status is not (InvoiceStatus.Paid or InvoiceStatus.Refunded))
                    {
                        invoice.Cancel();
                        affected++;
                    }

                    break;

                case ActionDuplicate:
                    var copy = invoice.Duplicate();
                    _context.Invoices.Add(copy);
                    affected++;
                    break;

                case ActionDelete:
                    if (invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
                    {
                        _context.Invoices.Remove(invoice);
                        affected++;
                    }

                    break;

                default:
                    return Result<int>.FailureCode(HostpanelErrorCodes.BULK_INVOICE_ACTION_UNKNOWN);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(affected);
    }
}
