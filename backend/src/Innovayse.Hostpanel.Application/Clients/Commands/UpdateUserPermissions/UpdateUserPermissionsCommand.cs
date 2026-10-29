using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateUserPermissions;

/// <summary>Command to update a non-owner user's permissions on a client account.</summary>
/// <param name="ClientId">The client account primary key.</param>
/// <param name="MemberUserId">The Identity user ID whose permissions to update.</param>
/// <param name="Permissions">New permissions as flags; the wire also accepts the numeric bit mask.</param>
public sealed record UpdateUserPermissionsCommand(int ClientId, string MemberUserId, ClientPermission Permissions);
