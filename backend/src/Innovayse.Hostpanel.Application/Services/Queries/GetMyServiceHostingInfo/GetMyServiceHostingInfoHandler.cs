using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMyServiceHostingInfo;

/// <summary>
/// Reads the live nameservers and certificate of the domain on one of the calling client's own
/// services, refusing every service that is not theirs.
/// </summary>
public sealed class GetMyServiceHostingInfoHandler
{
    /// <summary>The rule that says a client may only look at their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the nameservers and certificate of a domain over the network.</summary>
    private readonly IHostingProbe _probe;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only look at their own services.</param>
    /// <param name="context">The module's data context.</param>
    /// <param name="probe">Reads the nameservers and certificate of a domain over the network.</param>
    public GetMyServiceHostingInfoHandler(IServiceOwnership ownership, IHostpanelDbContext context, IHostingProbe probe)
    {
        _ownership = ownership;
        _context = context;
        _probe = probe;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. It names no account and no domain: both come from the service.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The nameservers and certificate of the service's domain (empty when the service has no domain
    /// or the probe read nothing), or <c>SERVICE_NOT_FOUND</c> when the service is not the caller's,
    /// when no such service exists, or when the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<HostingInfoDto>> HandleAsync(GetMyServiceHostingInfoQuery query, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.ServiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<HostingInfoDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var domain = await _context.ClientServices
            .AsNoTracking()
            .Where(s => s.Id == query.ServiceId)
            .Select(s => s.Domain)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(domain))
        {
            return Result<HostingInfoDto>.Success(HostingInfoDto.Empty);
        }

        return Result<HostingInfoDto>.Success(await _probe.ProbeAsync(new HostingProbeInput(domain.Trim()), ct));
    }
}
