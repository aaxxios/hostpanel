using System.Globalization;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoices;

/// <summary>Handles <see cref="AgingInvoicesQuery"/>.</summary>
public sealed class AgingInvoicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public AgingInvoicesHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// One page of the open invoices that still owe money, oldest due date first, each with the
    /// amount due in its own currency and its aging period. Never refuses.
    /// </returns>
    public async Task<Result<PaginatedList<AgingInvoiceDto>>> HandleAsync(AgingInvoicesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var balances = _context.Invoices.AsNoTracking().SelectAgedBalances(today);

        var totalCount = await balances.CountAsync(ct);

        var rows = await balances
            .OrderBy(b => b.DueDate)
            .ThenBy(b => b.InvoiceId)
            .Paginate(query.Page, query.PageSize)
            .Join(_context.Clients.AsNoTracking(), b => b.ClientId, c => c.Id,
                (b, c) => new { Balance = b, ClientName = c.FirstName + " " + c.LastName })
            .ToListAsync(ct);

        var items = rows.Select(x =>
        {
            var due = DateOnly.FromDateTime(x.Balance.DueDate.UtcDateTime);
            return new AgingInvoiceDto(
                InvoiceId: x.Balance.InvoiceId,
                Client: x.ClientName,
                Currency: x.Balance.Currency,
                AmountDue: x.Balance.AmountDue,
                DueDate: due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DaysOutstanding: Math.Max(0, today.DayNumber - due.DayNumber),
                Bucket: x.Balance.Bucket);
        }).ToList();

        return Result<PaginatedList<AgingInvoiceDto>>.Success(
            new PaginatedList<AgingInvoiceDto>(items, totalCount, query.Page, query.PageSize));
    }
}
