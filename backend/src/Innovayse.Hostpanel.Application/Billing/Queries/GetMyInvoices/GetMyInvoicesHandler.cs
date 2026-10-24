using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoices;

/// <summary>Returns the calling client's invoices, newest first. A draft is still being prepared by staff and is not shown.</summary>
public sealed class GetMyInvoicesHandler
{
    /// <summary>The module's data context, for resolving the caller's client record and their invoices.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for resolving the caller's client record and their invoices.</param>
    /// <param name="caller">Who is asking; the query does not say, and must not.</param>
    public GetMyInvoicesHandler(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <summary>
    /// Handles <see cref="GetMyInvoicesQuery"/>.
    /// </summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with all invoices for the client, their line items and transactions included,
    /// ordered newest first, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client
    /// record.
    /// </returns>
    public async Task<Result<IReadOnlyList<InvoiceDto>>> HandleAsync(GetMyInvoicesQuery query, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<IReadOnlyList<InvoiceDto>>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        // Was IInvoiceRepository.ListByClientAsync -- read-only, so no tracking is needed.
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Transactions)
            .Where(x => x.ClientId == client.Id && x.Status != InvoiceStatus.Draft)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        IReadOnlyList<InvoiceDto> dtos = invoices.Select(inv => InvoiceMapper.MapToDto(inv)).ToList();

        return Result.Success(dtos);
    }
}
