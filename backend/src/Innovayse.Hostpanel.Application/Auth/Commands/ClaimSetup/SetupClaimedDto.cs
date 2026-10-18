namespace Innovayse.Hostpanel.Application.Auth.Commands.ClaimSetup;

/// <summary>The acknowledgement the setup endpoint answers with once the role has been granted.</summary>
/// <param name="Success">Always true; a refusal is an error response instead.</param>
public sealed record SetupClaimedDto(bool Success);
