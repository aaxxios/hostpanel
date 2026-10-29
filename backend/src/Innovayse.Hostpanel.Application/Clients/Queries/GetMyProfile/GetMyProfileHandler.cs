using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;

/// <summary>
/// Returns the profile of the client record the authenticated user owns. The profile carries the
/// owner's sign-in address and two-factor state, so a user linked to the account never reads it.
/// </summary>
public sealed class GetMyProfileHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the person this client belongs to.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Resolves the account the caller owns; the query does not say whose, and must not.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the person this client belongs to.</param>
    /// <param name="access">Resolves the account the caller owns; the query does not say whose, and must not.</param>
    public GetMyProfileHandler(IHostpanelDbContext context, IIdentityProvider identity, IClientAccess access)
    {
        _context = context;
        _identity = identity;
        _access = access;
    }

    /// <summary>Retrieves the client profile for the authenticated user.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The caller's own client profile, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller owns no
    /// client record, which is also the answer for a user linked to somebody else's account.
    /// </returns>
    public async Task<Result<MyProfileDto>> HandleAsync(GetMyProfileQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireOwnClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<MyProfileDto>.FailureCode(clientId.ErrorCode!);
        }

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId.Value, ct);
        if (client is null)
        {
            return Result<MyProfileDto>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        var user = await _identity.FindBySubjectAsync(client.UserId, ct);
        var email = user?.Email ?? "";

        return Result<MyProfileDto>.Success(client.ToMyProfileDto(email, user?.TwoFactorEnabled ?? false, user?.Language));
    }
}
