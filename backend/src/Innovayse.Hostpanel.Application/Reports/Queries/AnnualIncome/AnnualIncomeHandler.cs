using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AnnualIncome;

/// <summary>Handles <see cref="AnnualIncomeQuery"/>.</summary>
public sealed class AnnualIncomeHandler
{
    /// <summary>Three-letter month names, indexed 0 for January.</summary>
    private static readonly string[] MonthNames =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public AnnualIncomeHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The year to report on; defaults to the current UTC year.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Twelve rows, January first, zero-filled for months with no income, in the base currency. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<AnnualIncomeDto>>> HandleAsync(AnnualIncomeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var year = query.Year ?? _clock.UtcNow.UtcDateTime.Year;

        var income = await _context.GetYearIncomeAsync(year, ct);

        IReadOnlyList<AnnualIncomeDto> rows = [.. income.Months.Select((amount, index) =>
            new AnnualIncomeDto(MonthNames[index], amount, income.Currency))];

        return Result<IReadOnlyList<AnnualIncomeDto>>.Success(rows);
    }
}
