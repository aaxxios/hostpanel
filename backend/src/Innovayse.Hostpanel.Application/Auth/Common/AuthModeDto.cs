namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>The sign-in mechanism this deployment runs.</summary>
/// <param name="Mode">Either <c>local</c> or <c>sso</c>.</param>
public sealed record AuthModeDto(string Mode);
