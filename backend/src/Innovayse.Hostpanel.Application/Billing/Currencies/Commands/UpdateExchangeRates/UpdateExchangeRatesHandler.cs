using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpdateExchangeRates;

/// <summary>
/// Refreshes every non-base currency's rate from the upstream rate source. A currency the source
/// does not quote keeps the rate it had and is reported back as missing rather than zeroed.
/// </summary>
public sealed class UpdateExchangeRatesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Where today's rates are read from.</summary>
    private readonly IExchangeRateSource _source;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="source">Where today's rates are read from.</param>
    public UpdateExchangeRatesHandler(IHostpanelDbContext context, IExchangeRateSource source)
    {
        _context = context;
        _source = source;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The command; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Which currencies were updated and which the source did not quote, or
    /// <c>BASE_CURRENCY_NOT_CONFIGURED</c> when no currency is marked as the base.
    /// </returns>
    public async Task<Result<UpdateExchangeRatesResultDto>> HandleAsync(
        UpdateExchangeRatesCommand cmd, CancellationToken ct)
    {
        var all = await _context.Currencies.ToListAsync(ct);

        var baseCurrency = all.Find(c => c.IsBase);
        if (baseCurrency is null)
        {
            return Result<UpdateExchangeRatesResultDto>.FailureCode(
                HostpanelErrorCodes.BASE_CURRENCY_NOT_CONFIGURED);
        }

        var others = all.Where(c => !c.IsBase).ToList();
        var rates = await _source.RatesToAsync(baseCurrency.Code, others.Select(c => c.Code), ct);

        List<string> updated = [];
        List<string> missing = [];

        foreach (var currency in others)
        {
            if (rates.TryGetValue(currency.Code, out var rate))
            {
                currency.UpdateRate(rate);
                updated.Add(currency.Code);
            }
            else
            {
                missing.Add(currency.Code);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result<UpdateExchangeRatesResultDto>.Success(new UpdateExchangeRatesResultDto(updated, missing));
    }
}
