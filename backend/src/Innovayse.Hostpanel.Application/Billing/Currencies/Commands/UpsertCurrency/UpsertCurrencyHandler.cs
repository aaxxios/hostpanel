using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpsertCurrency;

/// <summary>Creates a currency, or rewrites the one already under that code.</summary>
public sealed class UpsertCurrencyHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpsertCurrencyHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The currency to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success. This use case refuses nothing: an unknown code creates the currency.</returns>
    public async Task<Result> HandleAsync(UpsertCurrencyCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var code = cmd.Code!.ToUpperInvariant();
        var row = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == code, ct);

        if (row is null)
        {
            row = Currency.Create(
                code, cmd.Numeric, cmd.Prefix, cmd.Suffix, cmd.Decimals, cmd.RateToBase, isBase: false);
            _context.Currencies.Add(row);
        }
        else
        {
            row.UpdateDisplay(cmd.Prefix, cmd.Suffix, cmd.Decimals);

            // The base is worth one of itself by definition; the form's figure for it is noise,
            // and the domain would refuse it.
            if (!row.IsBase)
            {
                row.UpdateRate(cmd.RateToBase);
            }
        }

        ApplySwitch(row, cmd.IsEnabled);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Turns the currency on or off, whichever the command asked for.</summary>
    /// <param name="currency">The currency written by this command.</param>
    /// <param name="enabled">Whether it should be offered.</param>
    private static void ApplySwitch(Currency currency, bool enabled)
    {
        if (enabled)
        {
            currency.Enable();
        }
        else
        {
            currency.Disable();
        }
    }
}
