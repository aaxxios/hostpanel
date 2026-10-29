using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SendDomainExpiryWarning;

/// <summary>
/// Handles <see cref="SendDomainExpiryWarningCommand"/>: lets the domain pick the warning due and
/// raise <see cref="DomainExpiringEvent"/> through <c>RemindOfExpiry</c>, then saves. The event is
/// published only once the save commits, so a failed save never leaves a sent mail without its record.
/// </summary>
public sealed class SendDomainExpiryWarningHandler
{
    /// <summary>The data context the domain is loaded and the reminder saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SendDomainExpiryWarningHandler> _logger;

    /// <summary>The clock that dates the warning.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the domain is loaded and the reminder saved through.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="clock">The clock that dates the warning.</param>
    public SendDomainExpiryWarningHandler(
        IHostpanelDbContext context, ILogger<SendDomainExpiryWarningHandler> logger, IClock clock)
    {
        _context = context;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Records the warning the domain is due, if any.</summary>
    /// <param name="cmd">The domain.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, including when no warning was due; <c>DOMAIN_NOT_FOUND</c> when the id names no domain.</returns>
    public async Task<Result> HandleAsync(SendDomainExpiryWarningCommand cmd, CancellationToken ct)
    {
        // Tracked, not AsNoTracking: the reminder is added to the domain and saved below.
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.RemindOfExpiry($"client-{domain.ClientId}", _clock.UtcNow))
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Raised expiry warning for domain {DomainId}.", domain.Id);
        }

        return Result.Success();
    }
}
