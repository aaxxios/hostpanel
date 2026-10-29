using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceNotes;

/// <summary>Handles <see cref="UpdateServiceNotesCommand"/>: replaces the service's internal notes.</summary>
public sealed class UpdateServiceNotesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateServiceNotesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Replaces the service's internal notes.</summary>
    /// <param name="cmd">The new notes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service.</returns>
    public async Task<Result> HandleAsync(UpdateServiceNotesCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        service.UpdateAdminNotes(cmd.AdminNotes);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
