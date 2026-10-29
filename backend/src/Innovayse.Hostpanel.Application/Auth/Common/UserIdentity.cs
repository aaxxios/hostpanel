namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// The two facts about a locally stored account that sign-in and password reset need: who it is
/// and where to write to it.
/// </summary>
/// <param name="Id">The account's identifier; it becomes the subject of any credential issued for it.</param>
/// <param name="Email">The address the account signs in with.</param>
public sealed record UserIdentity(string Id, string Email);
