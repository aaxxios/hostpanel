using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// One invitation to join the account, as the <c>invitations</c> section of a client data export lists
/// it. The secret acceptance token is deliberately left out of the download.
/// </summary>
/// <param name="Id">The invitation's primary key.</param>
/// <param name="Email">Address the invitation was sent to.</param>
/// <param name="FirstName">Given name of the invitee.</param>
/// <param name="LastName">Family name of the invitee.</param>
/// <param name="Permissions">What the invitee would be allowed to do on the account.</param>
/// <param name="CreatedAt">When the invitation was sent.</param>
/// <param name="ExpiresAt">When the invitation stops being valid.</param>
/// <param name="AcceptedAt">When it was accepted, or null while it is still pending or lapsed.</param>
public sealed record ClientExportInvitationDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    ClientPermission Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt);
