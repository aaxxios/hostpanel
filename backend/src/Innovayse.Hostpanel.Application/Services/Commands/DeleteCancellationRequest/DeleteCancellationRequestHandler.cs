using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.DeleteCancellationRequest;

/// <summary>Deletes a cancellation request by primary key.</summary>
public sealed class DeleteCancellationRequestHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    public DeleteCancellationRequestHandler(IHostpanelDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The cancellation request to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>CANCELLATION_REQUEST_NOT_FOUND</c> when the id names no request.</returns>
    public async Task<Result> HandleAsync(DeleteCancellationRequestCommand cmd, CancellationToken ct)
    {
        // Was ICancellationRequestRepository.FindByIdAsync. Tracked, not AsNoTracking: the entity
        // is removed below and saved in this same pass.
        var request = await _context.CancellationRequests.FirstOrDefaultAsync(r => r.Id == cmd.Id, ct);
        if (request is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CANCELLATION_REQUEST_NOT_FOUND);
        }

        _context.CancellationRequests.Remove(request);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
