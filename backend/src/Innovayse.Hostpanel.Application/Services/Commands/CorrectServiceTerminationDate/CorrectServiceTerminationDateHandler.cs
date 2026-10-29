using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.CorrectServiceTerminationDate;

/// <summary>Handles <see cref="CorrectServiceTerminationDateCommand"/>: corrects the termination date of a terminated service.</summary>
public sealed class CorrectServiceTerminationDateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CorrectServiceTerminationDateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Corrects the termination date.</summary>
    /// <param name="cmd">The service and the moment it really ended.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service, or <c>SERVICE_NOT_TERMINATED</c>
    /// when the service is not terminated and so has no termination date.
    /// </returns>
    public async Task<Result> HandleAsync(CorrectServiceTerminationDateCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (service.Status is not ServiceStatus.Terminated)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_TERMINATED);
        }

        service.CorrectTerminationDate(cmd.TerminatedAt);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
