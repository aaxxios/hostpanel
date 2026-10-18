using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoices;

/// <summary>Handles <see cref="AgingInvoicesQuery"/>.</summary>
public sealed class AgingInvoicesHandler
{
    /// <summary>Statuses counted as still owed.</summary>
    private static readonly InvoiceStatus[] UnpaidStatuses = [InvoiceStatus.Unpaid, InvoiceStatus.Overdue];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AgingInvoicesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every unpaid or overdue invoice with its aging bucket. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<AgingInvoiceDto>>> HandleAsync(AgingInvoicesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rows = await _context.Invoices
            .AsNoTracking()
            .Where(i => UnpaidStatuses.Contains(i.Status))
            .Join(_context.Clients.AsNoTracking(), inv => inv.ClientId, c => c.Id,
                (inv, c) => new { inv, ClientName = c.FirstName + " " + c.LastName })
            .OrderBy(x => x.inv.DueDate)
            .ToListAsync(ct);

        IReadOnlyList<AgingInvoiceDto> result = rows.Select(x =>
        {
            var due = DateOnly.FromDateTime(x.inv.DueDate.DateTime);
            var days = today.DayNumber - due.DayNumber;
            var bucket = days <= 30 ? "0-30" : days <= 60 ? "31-60" : days <= 90 ? "61-90" : "90+";
            return new AgingInvoiceDto(
                InvoiceId: x.inv.Id,
                Client: x.ClientName,
                Amount: x.inv.Total,
                DueDate: due.ToString("yyyy-MM-dd"),
                DaysOutstanding: Math.Max(0, days),
                Bucket: bucket);
        }).ToList();

        return Result<IReadOnlyList<AgingInvoiceDto>>.Success(result);
    }
}
