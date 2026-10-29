using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoices;

/// <summary>Returns the invoices of the account the caller acts for, newest first. A draft is still being prepared by staff and is not shown.</summary>
public sealed class GetMyInvoicesHandler
{
    /// <summary>The module's data context, for the caller's invoices.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user; the query does not say, and must not.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the caller's invoices.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user; the query does not say, and must not.</param>
    public GetMyInvoicesHandler(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <summary>
    /// Handles <see cref="GetMyInvoicesQuery"/>.
    /// </summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with all invoices for the client, their line items and transactions included,
    /// ordered newest first, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller acts for no
    /// account.
    /// </returns>
    public async Task<Result<IReadOnlyList<MyInvoiceDto>>> HandleAsync(GetMyInvoicesQuery query, CancellationToken ct)
    {
        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<IReadOnlyList<MyInvoiceDto>>.FailureCode(clientId.ErrorCode!);
        }

        // Read-only, so no tracking is needed.
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Transactions)
            .Where(Invoice.ClientVisibleFilter)
            .Where(x => x.ClientId == clientId.Value)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        IReadOnlyList<MyInvoiceDto> dtos = invoices.Select(InvoiceMapper.MapToMyDto).ToList();

        return Result.Success(dtos);
    }
}
