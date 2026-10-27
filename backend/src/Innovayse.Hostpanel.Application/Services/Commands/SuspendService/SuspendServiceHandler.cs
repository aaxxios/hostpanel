using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.SuspendService;
/// <summary>Suspends an active client service. Provisioning is handled asynchronously via domain events.</summary>
public sealed class SuspendServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    public SuspendServiceHandler(IHostpanelDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The service to suspend.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service.</returns>
    public async Task<Result> HandleAsync(SuspendServiceCommand cmd, CancellationToken ct)
    {
        // Was IClientServiceRepository.FindByIdAsync. Tracked, not AsNoTracking: the service is
        // mutated and saved below.
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        service.Suspend(source: cmd.Source);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
