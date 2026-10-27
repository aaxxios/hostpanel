using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetVatMoss;

/// <summary>Handles <see cref="GetVatMossQuery"/>.</summary>
public sealed class GetVatMossHandler
{
    /// <summary>Calendar months in a quarter, used to derive the quarter today falls in.</summary>
    private const int MonthsPerQuarter = 3;

    /// <summary>Two-letter ISO country codes mapped to display names for the report's EU/EEA and common trading-partner countries.</summary>
    private static readonly Dictionary<string, string> CountryNames = new()
    {
        ["AT"] = "Austria",
        ["BE"] = "Belgium",
        ["BG"] = "Bulgaria",
        ["CY"] = "Cyprus",
        ["CZ"] = "Czech Republic",
        ["DE"] = "Germany",
        ["DK"] = "Denmark",
        ["EE"] = "Estonia",
        ["ES"] = "Spain",
        ["FI"] = "Finland",
        ["FR"] = "France",
        ["GB"] = "United Kingdom",
        ["GR"] = "Greece",
        ["HR"] = "Croatia",
        ["HU"] = "Hungary",
        ["IE"] = "Ireland",
        ["IT"] = "Italy",
        ["LT"] = "Lithuania",
        ["LU"] = "Luxembourg",
        ["LV"] = "Latvia",
        ["MT"] = "Malta",
        ["NL"] = "Netherlands",
        ["PL"] = "Poland",
        ["PT"] = "Portugal",
        ["RO"] = "Romania",
        ["SE"] = "Sweden",
        ["SI"] = "Slovenia",
        ["SK"] = "Slovakia",
        ["AM"] = "Armenia",
        ["RU"] = "Russia",
        ["US"] = "United States",
        ["UA"] = "Ukraine",
        ["TR"] = "Turkey",
    };

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetVatMossHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Year and quarter to report on; either may be omitted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>VAT collected per member state for the quarter. Never refuses.</returns>
    public async Task<Result<VatMossDto>> HandleAsync(GetVatMossQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The period defaults here rather than at the endpoint so every caller of the query,
        // HTTP or not, lands on the same quarter.
        var now = DateTime.UtcNow;
        var year = query.Year ?? now.Year;
        var quarter = query.Quarter ?? (((now.Month - 1) / MonthsPerQuarter) + 1);

        var (startMonth, endMonth) = quarter switch
        {
            1 => (1, 3),
            2 => (4, 6),
            3 => (7, 9),
            _ => (10, 12),
        };
        var from = new DateTimeOffset(new DateTime(year, startMonth, 1), TimeSpan.Zero);
        var to = new DateTimeOffset(
            new DateTime(year, endMonth, DateTime.DaysInMonth(year, endMonth), 23, 59, 59), TimeSpan.Zero);

        string[] monthNames =
        [
            string.Empty, "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December",
        ];
        var periodLabel = $"For Period 1st {monthNames[startMonth]} {year} to " +
                           $"{DateTime.DaysInMonth(year, endMonth)}th {monthNames[endMonth]} {year}";

        var raw = await _context.Invoices.AsNoTracking()
            .Where(inv => inv.Status == InvoiceStatus.Paid
                && inv.Tax > 0
                && inv.PaidAt != null
                && inv.PaidAt >= from
                && inv.PaidAt <= to)
            .Join(_context.Clients, inv => inv.ClientId, c => c.Id,
                (inv, c) => new { c.Country, inv.TaxRate, inv.SubTotal, inv.Tax })
            .Where(x => x.Country != null)
            .ToListAsync(ct);

        var grouped = raw
            .GroupBy(x => new { x.Country, x.TaxRate })
            .Select(g => new VatMossRowDto(
                CountryName(g.Key.Country!),
                g.Key.Country!,
                g.Key.TaxRate,
                g.Count(),
                g.Sum(x => x.SubTotal),
                g.Sum(x => x.Tax),
                "USD"))
            .OrderBy(r => r.CountryName)
            .ToList();

        return Result<VatMossDto>.Success(new VatMossDto(periodLabel, grouped));
    }

    /// <summary>Resolves a two-letter country code to its display name.</summary>
    /// <param name="code">The ISO country code stored on the client.</param>
    /// <returns>The display name, or the code itself when it is not in the known list.</returns>
    private static string CountryName(string code) => CountryNames.GetValueOrDefault(code, code);
}
