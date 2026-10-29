using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceAutomation;

/// <summary>
/// Handles <see cref="UpdateServiceAutomationCommand"/>: sets manual or automatic billing, the suspension
/// hold and the end-of-cycle termination.
/// </summary>
public sealed class UpdateServiceAutomationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateServiceAutomationHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Applies the automation settings to the service.</summary>
    /// <param name="cmd">The automation settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service. A hold date without the hold
    /// or a reason without the termination is refused by the validator before this runs, and by the
    /// aggregate (<c>SERVICE_AUTOMATION_INCONSISTENT</c>) should it ever get this far.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateServiceAutomationCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        service.ConfigureAutomation(
            cmd.AutoRenew, cmd.OverrideAutoSuspend, cmd.SuspendUntil, cmd.AutoTerminateEndOfCycle, cmd.AutoTerminateReason);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
