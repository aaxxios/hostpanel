using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Commands.AutoRenewDomains;

/// <summary>
/// Scheduled handler that auto-renews all eligible domains expiring within the next 30 days.
/// Failures per domain are logged and swallowed so a single failed renewal does not abort the batch.
/// </summary>
public sealed class AutoRenewDomainsHandler
{
    /// <summary>The data context the auto-renew-due domains are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus used to dispatch each domain's renewal.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<AutoRenewDomainsHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the auto-renew-due domains are read from.</param>
    /// <param name="bus">Wolverine bus used to dispatch each domain's renewal.</param>
    /// <param name="logger">Structured logger.</param>
    public AutoRenewDomainsHandler(IHostpanelDbContext context, IMessageBus bus, ILogger<AutoRenewDomainsHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="AutoRenewDomainsCommand"/>.
    /// </summary>
    /// <param name="cmd">The auto renew domains command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(AutoRenewDomainsCommand cmd, CancellationToken ct)
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(30);
        var domains = await _context.Domains
            .IncludeChildren()
            .Where(x => x.AutoRenew && x.ExpiresAt < threshold && x.Status == DomainStatus.Active)
            .ToListAsync(ct);

        foreach (var domain in domains)
        {
            try
            {
                var outcome = await _bus.InvokeAsync<Result>(new RenewDomainCommand(domain.Id, 1), ct);
                if (outcome.IsFailure)
                {
                    _logger.LogError(
                        "Auto-renewal failed for domain {DomainId} ({DomainName}): {ErrorCode}.",
                        domain.Id,
                        domain.Name,
                        outcome.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Auto-renewal failed for domain {DomainId} ({DomainName}).",
                    domain.Id,
                    domain.Name);
            }
        }
    }
}
