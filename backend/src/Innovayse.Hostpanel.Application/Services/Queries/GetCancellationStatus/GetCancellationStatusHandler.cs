using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Services.Queries.GetCancellationStatus;

/// <summary>Returns the cancellation status for a given client service.</summary>
public sealed class GetCancellationStatusHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetCancellationStatusHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the query. Checks for an existing open cancellation request on the service.
    /// </summary>
    /// <param name="qry">The service to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, always: a service with no open request answers "not pending".</returns>
    public async Task<Result<CancellationStatusDto>> HandleAsync(
        GetCancellationStatusQuery qry, CancellationToken ct)
    {
        // Was ICancellationRequestRepository.FindByServiceIdAsync -- read-only, so no tracking is
        // needed.
        var request = await _context.CancellationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ServiceId == qry.ServiceId, ct);

        if (request is null || request.Status != CancellationStatus.Open)
        {
            return Result<CancellationStatusDto>.Success(new CancellationStatusDto(false, null));
        }

        // The enum member name, not a sentence. This field is read by the client portal, which
        // ships en/ru/hy and resolves the member name to one of its own i18n keys; English prose
        // baked in here cannot be translated. It is also the same spelling CancelServiceCommand
        // accepts on the way in, so what a caller reads back is what it may send.
        return Result<CancellationStatusDto>.Success(new CancellationStatusDto(true, request.Type.ToString()));
    }
}
