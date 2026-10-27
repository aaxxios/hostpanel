using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncPendingDomainStatuses;

/// <summary>
/// Polls the registrar for every domain currently in <see cref="DomainStatus.PendingRegistration"/>
/// or <see cref="DomainStatus.PendingTransfer"/> status and activates it if the registrar reports
/// the domain as live.
/// </summary>
/// <remarks>
/// This is needed for registrars like Name.am where cart purchases are processed asynchronously
/// and the domain is not immediately active after the API call.
/// </remarks>
public sealed class SyncPendingDomainStatusesHandler
{
    /// <summary>Interval in minutes between consecutive sync runs.</summary>
    private const int IntervalMinutes = 15;

    /// <summary>The data context the pending domains are read from and the activation is saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The registrar provider polled for each pending domain's live status.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>Unit of work the activated domain is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Wolverine bus used to re-schedule the next run.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SyncPendingDomainStatusesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the pending domains are read from and the activation is saved through.</param>
    /// <param name="registrar">The registrar provider polled for each pending domain's live status.</param>
    /// <param name="uow">Unit of work the activated domain is saved through.</param>
    /// <param name="bus">Wolverine bus used to re-schedule the next run.</param>
    /// <param name="logger">Structured logger.</param>
    public SyncPendingDomainStatusesHandler(
        IHostpanelDbContext context,
        IRegistrarProvider registrar,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<SyncPendingDomainStatusesHandler> logger)
    {
        _context = context;
        _registrar = registrar;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="SyncPendingDomainStatusesCommand"/>.
    /// Iterates all pending domains, polls the registrar, and activates any that are now live.
    /// Individual domain failures are logged and swallowed so one error does not abort the batch.
    /// Re-schedules itself for the next run.
    /// </summary>
    /// <param name="cmd">The sync pending domain statuses command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleAsync(SyncPendingDomainStatusesCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return RecurringJob.RunAsync(
            nameof(SyncPendingDomainStatusesCommand), () => RunAsync(ct), ScheduleNextAsync, _logger, ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJob"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        var pending = await _context.Domains
            .Where(x => x.Status == DomainStatus.PendingRegistration || x.Status == DomainStatus.PendingTransfer)
            .OrderBy(x => x.RegisteredAt)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            _logger.LogDebug("SyncPendingDomainStatuses: no pending domains found.");
        }
        else
        {
            _logger.LogInformation(
                "SyncPendingDomainStatuses: checking {Count} pending domain(s).", pending.Count);

            foreach (var domain in pending)
            {
                try
                {
                    await SyncDomainAsync(domain, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "SyncPendingDomainStatuses: failed to sync domain {DomainName} (ID {DomainId}).",
                        domain.Name, domain.Id);
                }
            }
        }
    }

    /// <summary>Schedules the next pass.</summary>
    /// <returns>A task.</returns>
    private async Task ScheduleNextAsync()
    {
        var next = DateTimeOffset.UtcNow.AddMinutes(IntervalMinutes);
        await _bus.ScheduleAsync(new SyncPendingDomainStatusesCommand(), next);
        _logger.LogDebug("SyncPendingDomainStatuses: next run scheduled for {Next:u}.", next);
    }

    /// <summary>
    /// Polls the registrar for a single domain and activates it if the registrar reports it live.
    /// </summary>
    /// <param name="domain">The pending domain aggregate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SyncDomainAsync(RegisteredDomain domain, CancellationToken ct)
    {
        var result = await _registrar.CheckDomainActiveAsync(domain.Name, ct);

        if (!result.Success)
        {
            _logger.LogDebug(
                "Domain {DomainName} (ID {DomainId}) not yet active: {Reason}",
                domain.Name, domain.Id, result.ErrorMessage);
            return;
        }

        var expiresAt = result.ExpiresAt ?? DateTimeOffset.UtcNow.AddYears(1);
        var registrarRef = result.RegistrarRef ?? domain.Name;

        if (domain.Status == DomainStatus.PendingRegistration)
        {
            domain.Activate(registrarRef);
            _logger.LogInformation(
                "Domain {DomainName} (ID {DomainId}) activated via polling (expires {ExpiresAt:u}).",
                domain.Name, domain.Id, expiresAt);
        }
        else if (domain.Status == DomainStatus.PendingTransfer)
        {
            domain.ActivateTransfer(registrarRef, expiresAt);
            _logger.LogInformation(
                "Domain {DomainName} (ID {DomainId}) transfer confirmed via polling (expires {ExpiresAt:u}).",
                domain.Name, domain.Id, expiresAt);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
