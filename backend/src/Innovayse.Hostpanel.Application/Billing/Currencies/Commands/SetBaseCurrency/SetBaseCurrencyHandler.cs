using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.SetBaseCurrency;

/// <summary>
/// Moves the base to another currency and re-expresses every other rate against it, so the stored
/// figures keep meaning the same amounts of money.
/// </summary>
public sealed class SetBaseCurrencyHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public SetBaseCurrencyHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The currency to make the base.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CURRENCY_NOT_FOUND</c> when the code names no configured currency.
    /// </returns>
    public async Task<Result> HandleAsync(SetBaseCurrencyCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Base-first, and that order is load-bearing: one filtered unique index allows a single
        // IsBase row, and EF writes its UPDATEs in the order the entities were tracked. Loaded in
        // any other order, the new base can be written before the old one is cleared, and Postgres
        // refuses the whole save.
        var all = await _context.Currencies
            .OrderByDescending(c => c.IsBase)
            .ThenBy(c => c.Code)
            .ToListAsync(ct);
        var code = cmd.Code!.ToUpperInvariant();

        var newBase = all.Find(c => c.Code == code);
        if (newBase is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
        }

        if (newBase.IsBase)
        {
            return Result.Success();
        }

        var newBaseInOldBase = newBase.RateToBase;
        newBase.MakeBase();

        foreach (var other in all.Where(c => c.Code != code))
        {
            if (other.IsBase)
            {
                other.UnmakeBase();
            }

            other.UpdateRate(other.RateToBase / newBaseInOldBase);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
