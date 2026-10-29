using Innovayse.Hostpanel.Application.Clients.Common.Enums;
using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;

/// <summary>
/// Command for a client inviting someone to their own account.
/// </summary>
/// <remarks>
/// The client portal sends a mode plus a list of single permissions, never the bit-flags integer
/// the admin endpoint takes: a browser that can post an arbitrary integer could grant a
/// combination the UI never offered, while every list entry here is checked on its own.
/// </remarks>
/// <param name="Email">The invitee's email address.</param>
/// <param name="FirstName">The invitee's first name.</param>
/// <param name="LastName">The invitee's last name.</param>
/// <param name="PermissionMode">Whether the invitee gets every permission or only the listed ones.</param>
/// <param name="Permissions">
/// The single permissions to grant when <paramref name="PermissionMode"/> is
/// <see cref="InvitePermissionMode.Choose"/>; empty for <see cref="InvitePermissionMode.All"/>.
/// </param>
public sealed record InviteMyUserCommand(
    string Email,
    string FirstName,
    string LastName,
    InvitePermissionMode PermissionMode,
    IReadOnlyList<ClientPermission> Permissions);
