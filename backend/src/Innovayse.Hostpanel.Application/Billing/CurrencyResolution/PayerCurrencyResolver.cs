using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.CurrencyResolution;

/// <summary>Resolves a payer's billing currency from their client record, falling back to the base.</summary>
public sealed class PayerCurrencyResolver : IPayerCurrencyResolver
{
    /// <summary>The module's data context, for the recorded client currency and the configured currencies.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The current request's caller.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for the recorded client currency and the configured currencies.</param>
    /// <param name="caller">The current request's caller.</param>
    public PayerCurrencyResolver(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<Currency> ForClientAsync(int clientId, CancellationToken ct)
    {
        // Was IClientRepository.FindByIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId, ct);
        return await ResolveAsync(client?.Currency, ct);
    }

    /// <inheritdoc/>
    public async Task<Currency> ForCallerAsync(CancellationToken ct)
    {
        if (_caller.UserId is not { } userId)
        {
            return await _context.GetBaseCurrencyAsync(ct);
        }

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        return await ResolveAsync(client?.Currency, ct);
    }

    /// <inheritdoc/>
    public Task<Currency> BaseAsync(CancellationToken ct) => _context.GetBaseCurrencyAsync(ct);

    /// <summary>
    /// Maps a recorded alpha code to its configured row, or to the base when there is none.
    /// </summary>
    /// <remarks>
    /// A client can carry a code that was never configured -- imported data, or a currency an
    /// admin later removed. Billing them in it would produce an invoice nothing can format or
    /// charge, so the base is used. The condition is worth an admin's attention, which is why
    /// the client form (Plan 2) flags a code that is not in the list.
    /// </remarks>
    /// <param name="recorded">The client's recorded code, or null.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The configured currency.</returns>
    private async Task<Currency> ResolveAsync(string? recorded, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(recorded))
        {
            return await _context.GetBaseCurrencyAsync(ct);
        }

        return await _context.FindCurrencyAsync(recorded, ct) ?? await _context.GetBaseCurrencyAsync(ct);
    }
}
