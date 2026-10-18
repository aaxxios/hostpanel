namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;

/// <summary>
/// Command for a client inviting someone to their own account.
/// </summary>
/// <remarks>
/// The client portal sends permissions as the literal <c>all</c> or a comma-separated list of
/// keys, never the bit-flags integer the admin endpoint takes: the numbers are an internal
/// encoding, and a browser that can post an arbitrary integer can grant a permission the UI never
/// offered. <see cref="InviteMyUserHandler"/> is the only place that knows both spellings.
/// </remarks>
/// <param name="Email">The invitee's email address.</param>
/// <param name="FirstName">The invitee's first name.</param>
/// <param name="LastName">The invitee's last name.</param>
/// <param name="Permissions">
/// The literal <c>all</c>, a comma-separated list of permission keys, or null. An unrecognised
/// key is dropped rather than refused, and nothing recognisable at all grants nothing.
/// </param>
public sealed record InviteMyUserCommand(string Email, string FirstName, string LastName, string? Permissions);
