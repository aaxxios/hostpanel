using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListCurrencies;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Common.Enums;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoicesSummary;

/// <summary>Handles <see cref="AgingInvoicesSummaryQuery"/>.</summary>
/// <remarks>
/// The amount owed per currency and period is summed in SQL; only the pivot into one row per
/// period with a column per currency is shaped here, because its column set depends on which
/// currencies are in play. The columns are every enabled currency (base first, as the currencies
/// page lists them) plus any other currency an open invoice still bills in.
/// </remarks>
public sealed class AgingInvoicesSummaryHandler
{
    /// <summary>Aging buckets, in display order.</summary>
    private static readonly AgingBucket[] Buckets = Enum.GetValues<AgingBucket>();

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>The bus the Billing module's currency list is read through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    /// <param name="bus">The bus the Billing module's currency list is read through.</param>
    public AgingInvoicesSummaryHandler(IHostpanelDbContext context, IClock clock, IMessageBus bus)
    {
        _context = context;
        _clock = clock;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Amounts still owed per aging period and invoice currency, plus the grand total per currency. Never refuses.</returns>
    public async Task<Result<AgingInvoiceSummaryDto>> HandleAsync(AgingInvoicesSummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        var sums = await _context.Invoices
            .AsNoTracking()
            .SelectAgedBalances(today)
            .GroupBy(b => new { b.Currency, b.Bucket })
            .Select(g => new { g.Key.Currency, g.Key.Bucket, Amount = g.Sum(b => b.AmountDue) })
            .ToListAsync(ct);

        var configured = await _bus.InvokeAsync<Result<IReadOnlyList<CurrencyDto>>>(
            new ListCurrenciesQuery(EnabledOnly: true), ct);
        var currencies = (configured.Value ?? [])
            .Select(c => c.Code)
            .Concat(sums.Select(s => s.Currency).Distinct().Order(StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var totals = currencies.ToDictionary(c => c, c => sums.Where(s => s.Currency == c).Sum(s => s.Amount), StringComparer.Ordinal);
        var periods = Buckets
            .Select(bucket => new AgingPeriodDto(
                bucket,
                currencies.ToDictionary(
                    c => c,
                    c => sums.Where(s => s.Currency == c && s.Bucket == bucket).Sum(s => s.Amount),
                    StringComparer.Ordinal)))
            .ToList();

        return Result<AgingInvoiceSummaryDto>.Success(new AgingInvoiceSummaryDto(periods, totals, currencies));
    }
}
