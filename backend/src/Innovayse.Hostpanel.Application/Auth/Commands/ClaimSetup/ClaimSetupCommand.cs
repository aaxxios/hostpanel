namespace Innovayse.Hostpanel.Application.Auth.Commands.ClaimSetup;

/// <summary>
/// Command for the first-run claim from the setup endpoint. Carries no subject: who is being made
/// Admin is the caller, resolved from the credential by the shared write.
/// </summary>
/// <param name="SetupToken">The token this installation printed to its log; ignored under <c>sso</c>.</param>
public sealed record ClaimSetupCommand(string? SetupToken);
