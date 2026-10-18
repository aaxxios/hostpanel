namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyUser;

/// <summary>Command for a client removing someone from their own account. Names no account: the handler resolves the caller.</summary>
/// <param name="MemberUserId">The Identity user id to remove.</param>
public sealed record RemoveMyUserCommand(string MemberUserId);
