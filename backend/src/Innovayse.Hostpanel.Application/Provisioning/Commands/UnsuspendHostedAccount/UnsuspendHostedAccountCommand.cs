namespace Innovayse.Hostpanel.Application.Provisioning.Commands.UnsuspendHostedAccount;

/// <summary>Re-activates a previously suspended hosting service on the provisioning provider.</summary>
/// <param name="ServiceId">The client service identifier to unsuspend. Overlaid from the route.</param>
public sealed record UnsuspendHostedAccountCommand(int ServiceId);
