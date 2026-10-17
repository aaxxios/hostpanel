using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoicesSummary;

/// <summary>Handles <see cref="AgingInvoicesSummaryQuery"/>.</summary>
/// <remarks>
/// The per-currency, per-period pivot below is a dynamic shape SQL cannot return directly (the
/// column set depends on which currencies are actually in play), so the unpaid invoices are
/// loaded once — a small, already-filtered set, never the whole invoices table — and pivoted in
/// memory exactly as the pre-conversion repository did.
/// </remarks>
public sealed class AgingInvoicesSummaryHandler
{
    /// <summary>Statuses counted as still owed.</summary>
    private static readonly InvoiceStatus[] UnpaidStatuses = [InvoiceStatus.Unpaid, InvoiceStatus.Overdue];

    /// <summary>Aging bucket labels, in display order.</summary>
    private static readonly string[] PeriodNames = ["0 - 30", "30 - 60", "60 - 90", "90 - 120", "120 +"];

    /// <summary>Currencies always shown even when no invoice currently uses them.</summary>
    private static readonly string[] DefaultCurrencies = ["USD", "EUR", "AMD", "RUB"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AgingInvoicesSummaryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Totals owed per aging period and currency, plus the grand total per currency. Never refuses.</returns>
    public async Task<Result<AgingInvoiceSummaryDto>> HandleAsync(AgingInvoicesSummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => UnpaidStatuses.Contains(i.Status))
            .Join(_context.Clients.AsNoTracking(), inv => inv.ClientId, c => c.Id,
                (inv, c) => new { inv.Total, inv.DueDate, Currency = c.Currency ?? "USD" })
            .ToListAsync(ct);

        var invoiceCurrencies = invoices.Select(x => x.Currency).Distinct();
        var currencies = DefaultCurrencies.Union(invoiceCurrencies).Distinct().OrderBy(c => c).ToList();

        var periods = new List<AgingPeriodDto>();
        var totals = new Dictionary<string, decimal>();
        foreach (var c in currencies)
        {
            totals[c] = 0m;
        }

        foreach (var periodName in PeriodNames)
        {
            var amounts = new Dictionary<string, decimal>();
            foreach (var c in currencies)
            {
                amounts[c] = 0m;
            }

            foreach (var inv in invoices)
            {
                var due = DateOnly.FromDateTime(inv.DueDate.DateTime);
                var days = today.DayNumber - due.DayNumber;
                var bucket = days <= 30 ? "0 - 30" : days <= 60 ? "30 - 60" : days <= 90 ? "60 - 90" : days <= 120 ? "90 - 120" : "120 +";
                if (bucket != periodName)
                {
                    continue;
                }

                amounts[inv.Currency] += inv.Total;
                totals[inv.Currency] += inv.Total;
            }

            periods.Add(new AgingPeriodDto(periodName, amounts));
        }

        return Result<AgingInvoiceSummaryDto>.Success(new AgingInvoiceSummaryDto(periods, totals, currencies));
    }
}
