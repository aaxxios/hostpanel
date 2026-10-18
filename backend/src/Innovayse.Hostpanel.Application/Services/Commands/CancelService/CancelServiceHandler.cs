using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Services.Commands.CancelService;

/// <summary>Creates a cancellation request for a client service.</summary>
public sealed class CancelServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    public CancelServiceHandler(IHostpanelDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The service to cancel, the cancellation type and an optional reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service, or
    /// <c>CANCELLATION_ALREADY_PENDING</c> when an open request already exists for it.
    /// </returns>
    public async Task<Result> HandleAsync(CancelServiceCommand cmd, CancellationToken ct)
    {
        // Was IClientServiceRepository.FindByIdAsync -- read-only here: the service is never
        // mutated by this handler, only checked to exist.
        var service = await _context.ClientServices
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        // Was ICancellationRequestRepository.FindByServiceIdAsync -- read-only, so no tracking is
        // needed.
        var existing = await _context.CancellationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ServiceId == cmd.ServiceId, ct);
        if (existing is not null && existing.Status == CancellationStatus.Open)
        {
            return Result.FailureCode(HostpanelErrorCodes.CANCELLATION_ALREADY_PENDING);
        }

        // Case-sensitive on purpose, matching CancelServiceValidator's ordinal
        // "Immediate" or "EndOfBillingPeriod" exactly. Strict is the right side to converge on
        // because Type is a machine token, not prose: GetCancellationStatusHandler hands back
        // this exact spelling, so a caller echoing what it read always matches, and accepting
        // "IMMEDIATE" would only invite a spelling that works here and nowhere else. The
        // validator already refuses every other spelling, so this parse never sees one.
        var cancellationType = Enum.Parse<CancellationType>(cmd.Type);
        var request = CancellationRequest.Create(service.Id, cancellationType, cmd.Reason);
        _context.CancellationRequests.Add(request);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
