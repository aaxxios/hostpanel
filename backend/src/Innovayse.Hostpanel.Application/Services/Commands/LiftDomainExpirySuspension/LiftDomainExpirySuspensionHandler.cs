using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Commands.LiftDomainExpirySuspension;

/// <summary>
/// Handles <see cref="LiftDomainExpirySuspensionCommand"/>: unsuspends the service only when it is
/// suspended because its domain expired. A manual or overdue suspension is not a renewal's to undo.
/// Provisioning follows through the service's domain events, as for any unsuspension.
/// </summary>
public sealed class LiftDomainExpirySuspensionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<LiftDomainExpirySuspensionHandler> _logger;

    /// <summary>Reads the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="clock">Reads the current moment.</param>
    public LiftDomainExpirySuspensionHandler(
        IHostpanelDbContext context, ILogger<LiftDomainExpirySuspensionHandler> logger,
        IClock clock)
    {
        _context = context;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The service linked to the renewed domain.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, also when there is nothing to lift (the service is not suspended, or was suspended for
    /// another reason); or <c>SERVICE_NOT_FOUND</c> when the id names no service.
    /// </returns>
    public async Task<Result> HandleAsync(LiftDomainExpirySuspensionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        // A null source is a suspension recorded before sources existed; renewal lifted those before,
        // so it still does.
        if (service.Status != ServiceStatus.Suspended
            || service.SuspensionSource is not (null or SuspensionSource.DomainExpired))
        {
            _logger.LogInformation(
                "Service {ServiceId} is {Status} (suspension source {Source}); nothing a domain renewal lifts.",
                service.Id, service.Status, service.SuspensionSource);
            return Result.Success();
        }

        service.Unsuspend(_clock.UtcNow);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Lifted the domain expiry suspension of service {ServiceId}.", service.Id);
        return Result.Success();
    }
}
