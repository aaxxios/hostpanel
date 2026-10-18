using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Authorization;

/// <summary>
/// Resolves invoice ownership against the client the current credential names.
/// </summary>
public sealed class InvoiceOwnership : IInvoiceOwnership
{
    /// <summary>The module's data context, for mapping the caller's subject to their account and reading the invoice.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose invoices to consider.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for mapping the caller's subject to their account and reading the invoice.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose invoices to consider.</param>
    public InvoiceOwnership(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int invoiceId, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is not null)
        {
            // Was IInvoiceRepository.FindByIdAsync -- read-only, so no tracking is needed.
            var invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == invoiceId, ct);
            if (invoice is not null && invoice.ClientId == client.Id)
            {
                return Result.Success();
            }
        }

        // An invoice that does not exist, an invoice belonging to somebody else, and a caller with
        // no client record all land here and answer identically. Distinguishing them would turn
        // this route into a way of enumerating ids -- and invoice ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }
}
