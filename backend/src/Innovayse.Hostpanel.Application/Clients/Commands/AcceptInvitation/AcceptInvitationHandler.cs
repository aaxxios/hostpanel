using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AcceptInvitation;

/// <summary>
/// Validates the invitation token, links the already-authenticated caller to the client account
/// with the invitation's permissions, and acknowledges it.
/// </summary>
public sealed class AcceptInvitationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Grants the caller the <see cref="Role.Client"/> role once they accept.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Who is accepting; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="roles">Grants the caller the <see cref="Role.Client"/> role once they accept.</param>
    /// <param name="caller">Who is accepting; the command does not say, and must not.</param>
    /// <param name="clock">The source of the current moment.</param>
    public AcceptInvitationHandler(IHostpanelDbContext context, ISubjectRoleStore roles, ICurrentUserService caller, IClock clock)
    {
        _context = context;
        _roles = roles;
        _caller = caller;
        _clock = clock;
    }

    /// <summary>
    /// Accepts the invitation, links the caller to the client, and acknowledges it.
    /// </summary>
    /// <param name="cmd">The accept invitation command, carrying the token only.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The acknowledgement, or <c>INVITATION_NOT_FOUND</c> when the token names no invitation,
    /// <c>INVITATION_EXPIRED</c> when it has passed its window, <c>INVITATION_ALREADY_ACCEPTED</c>
    /// when it was already used, or <c>CLIENT_NOT_FOUND</c> when the invitation's own client no
    /// longer exists.
    /// </returns>
    public async Task<Result<InvitationAcceptedDto>> HandleAsync(AcceptInvitationCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var userId = _caller.RequireUserId();

        var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Token == cmd.Token, ct);
        if (invitation is null)
        {
            return Result<InvitationAcceptedDto>.FailureCode(HostpanelErrorCodes.INVITATION_NOT_FOUND);
        }

        // Validated before it is marked accepted: a user-creation failure further down must not
        // leave a spent invitation behind.
        var now = _clock.UtcNow;
        if (invitation.IsExpiredAt(now))
        {
            return Result<InvitationAcceptedDto>.FailureCode(HostpanelErrorCodes.INVITATION_EXPIRED);
        }

        if (invitation.IsAccepted)
        {
            return Result<InvitationAcceptedDto>.FailureCode(HostpanelErrorCodes.INVITATION_ALREADY_ACCEPTED);
        }

        // The caller is already signed in, and this handler asked the credential who they are
        // rather than being told. Nothing here writes to their account: the invitation carries a
        // name and address only so the email that delivered it could be addressed, and copying
        // those over the account's own details would let whoever sent the invitation rename the
        // person who accepted it.
        await _roles.AddAsync(userId, Role.Client, ct);

        var client = await _context.Clients
            .IncludeChildren()
            .FirstOrDefaultAsync(c => c.Id == invitation.ClientId, ct);
        if (client is null)
        {
            return Result<InvitationAcceptedDto>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.AddUser(userId, invitation.Permissions);

        // Marked accepted only after everything else succeeded.
        invitation.Accept(now);

        await _context.SaveChangesAsync(ct);

        return Result<InvitationAcceptedDto>.Success(new InvitationAcceptedDto(true));
    }
}
