using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Services.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Commands.LinkDomainService;

/// <summary>Handles <see cref="LinkDomainServiceCommand"/>: loads the domain, links the service and saves.</summary>
public sealed class LinkDomainServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public LinkDomainServiceHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The domain and the service to link.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once linked, <c>DOMAIN_NOT_FOUND</c> when the id names no domain,
    /// <c>SERVICE_NOT_FOUND</c> when the service id names no service, or
    /// <c>DOMAIN_LINK_NOT_ALLOWED</c> when the domain is cancelled or transferred out.
    /// </returns>
    public async Task<Result> HandleAsync(LinkDomainServiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(d => d.Id == cmd.DomainId, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (!domain.CanLinkService)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_LINK_NOT_ALLOWED);
        }

        if (!await _bus.ServiceExistsAsync(cmd.ServiceId, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        domain.LinkService(cmd.ServiceId);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
