using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;

/// <summary>Finds the client record linked to the authenticated user and returns their profile.</summary>
public sealed class GetMyProfileHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the person this client belongs to.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Whose profile; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the person this client belongs to.</param>
    /// <param name="caller">Whose profile; the query does not say, and must not.</param>
    public GetMyProfileHandler(IHostpanelDbContext context, IIdentityProvider identity, ICurrentUserService caller)
    {
        _context = context;
        _identity = identity;
        _caller = caller;
    }

    /// <summary>Retrieves the client profile for the authenticated user.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The caller's own client profile, or <c>CLIENT_PROFILE_NOT_FOUND</c> when no client record
    /// exists for the caller.
    /// </returns>
    public async Task<Result<ClientDto>> HandleAsync(GetMyProfileQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userId = _caller.RequireUserId();

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (client is null)
        {
            return Result<ClientDto>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        var user = await _identity.FindBySubjectAsync(client.UserId, ct);
        var email = user?.Email ?? "";

        return Result<ClientDto>.Success(client.ToDto(email, user?.TwoFactorEnabled ?? false, user?.Language));
    }
}
