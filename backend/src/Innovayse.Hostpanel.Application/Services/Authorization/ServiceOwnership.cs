using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Authorization;

/// <summary>
/// Resolves client-service ownership against the client the current credential names.
/// </summary>
public sealed class ServiceOwnership : IServiceOwnership
{
    /// <summary>The module's data context, for the service and the caller's client record.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose services to consider.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for the service and the caller's client record.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose services to consider.</param>
    public ServiceOwnership(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int serviceId, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is not null)
        {
            // Was IClientServiceRepository.FindByIdAsync -- read-only, so no tracking is needed.
            var service = await _context.ClientServices
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == serviceId, ct);
            if (service is not null && service.ClientId == client.Id)
            {
                return Result.Success();
            }
        }

        // A service that does not exist, a service belonging to somebody else, and a caller with
        // no client record all land here and answer identically. Distinguishing them would turn
        // this route into a way of enumerating ids -- and service ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <inheritdoc/>
    public async Task<Result<int>> RequireCallerClientIdAsync(CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        return Result<int>.Success(client.Id);
    }
}
