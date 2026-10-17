namespace Innovayse.Hostpanel.Application.Provisioning.Commands.TerminateHostedAccount;

/// <summary>Permanently terminates a hosting service on the provisioning provider.</summary>
/// <param name="ServiceId">The client service identifier to terminate. Overlaid from the route.</param>
/// <param name="Reason">Human-readable reason for the termination.</param>
public sealed record TerminateHostedAccountCommand(int ServiceId, string Reason);
