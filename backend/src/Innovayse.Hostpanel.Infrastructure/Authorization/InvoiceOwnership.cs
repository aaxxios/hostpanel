using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Authorization;

/// <summary>
/// Resolves invoice ownership against the client account the current credential acts for.
/// </summary>
public sealed class InvoiceOwnership : IInvoiceOwnership
{
    /// <summary>The module's data context, for reading the invoice.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for reading the invoice.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public InvoiceOwnership(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int invoiceId, CancellationToken ct)
    {
        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsSuccess)
        {
            // Read-only, so no tracking is needed.
            var owned = await _context.Invoices.AsNoTracking()
                .Where(Invoice.ClientVisibleFilter)
                .AnyAsync(x => x.Id == invoiceId && x.ClientId == clientId.Value, ct);
            if (owned)
            {
                return Result.Success();
            }
        }

        // An invoice that does not exist, an invoice belonging to somebody else, a draft the caller
        // owns, and a caller with no account all land here and answer identically. Distinguishing them would turn
        // this route into a way of enumerating ids, and invoice ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }
}
