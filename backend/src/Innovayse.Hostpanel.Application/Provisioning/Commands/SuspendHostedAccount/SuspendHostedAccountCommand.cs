namespace Innovayse.Hostpanel.Application.Provisioning.Commands.SuspendHostedAccount;

/// <summary>Suspends an active hosting service on the provisioning provider.</summary>
/// <param name="ServiceId">The client service identifier to suspend. Overlaid from the route.</param>
/// <param name="Reason">Human-readable reason for the suspension (e.g. "Non-payment").</param>
public sealed record SuspendHostedAccountCommand(int ServiceId, string Reason);
