namespace Innovayse.Hostpanel.Application.Auth.Commands.BackchannelLogout;

/// <summary>
/// Command raised when the sign-on service tells this product that a person has signed out
/// everywhere: every session this product holds for that person must end.
/// </summary>
/// <remarks>
/// Carries an email rather than a subject because that is all the platform portal sends. It names
/// the person being signed out, not the caller (the caller is the portal, authenticated by a
/// shared key), so it cannot be spoofed into acting as someone.
/// </remarks>
/// <param name="Email">The address of the person who signed out.</param>
public sealed record BackchannelLogoutCommand(string? Email);
