using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Registrar;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetTldRenewPrice;

/// <summary>
/// Handles <see cref="GetTldRenewPriceQuery"/>: matches the name to a configured TLD the way the
/// registrar resolver does, longest suffix first ("example.co.am" is priced as "co.am" when that TLD
/// is configured, else as "am"), and reads its renew price for the period.
/// </summary>
public sealed class GetTldRenewPriceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetTldRenewPriceHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Reads the price.</summary>
    /// <param name="query">The domain name and the period.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The price (null when the period has none), or <c>TLD_CONFIG_NOT_FOUND</c> when no TLD of the name is configured.</returns>
    public async Task<Result<TldRenewPriceDto>> HandleAsync(GetTldRenewPriceQuery query, CancellationToken ct)
    {
        foreach (var tld in RegistrarProviderResolver.ExtractTldCandidates(query.DomainName))
        {
            var config = await _context.TldConfigs.AsNoTracking().FirstOrDefaultAsync(t => t.Tld == tld, ct);
            if (config is null)
            {
                continue;
            }

            decimal? price = config.SellRenew.TryGetValue(query.Years, out var sell) ? sell : null;
            return Result<TldRenewPriceDto>.Success(new TldRenewPriceDto(config.Tld, config.SellCurrency, price));
        }

        return Result<TldRenewPriceDto>.FailureCode(HostpanelErrorCodes.TLD_CONFIG_NOT_FOUND);
    }
}
