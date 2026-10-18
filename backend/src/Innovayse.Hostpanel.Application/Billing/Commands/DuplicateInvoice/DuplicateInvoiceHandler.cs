using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DuplicateInvoice;

/// <summary>Creates a draft copy of an existing invoice.</summary>
public sealed class DuplicateInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DuplicateInvoiceHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The duplicate command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the ID of the newly created draft invoice, or <c>INVOICE_NOT_FOUND</c> when the
    /// source invoice does not exist.
    /// </returns>
    public async Task<Result<int>> HandleAsync(DuplicateInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var source = await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (source is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        var copy = source.Duplicate();
        _context.Invoices.Add(copy);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(copy.Id);
    }
}
