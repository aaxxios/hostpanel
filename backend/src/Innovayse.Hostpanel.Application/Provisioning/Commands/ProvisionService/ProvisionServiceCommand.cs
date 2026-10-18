namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ProvisionService;

/// <summary>Provisions a pending hosting service on the configured provider.</summary>
/// <param name="ServiceId">The client service identifier to provision. Overlaid from the route.</param>
public sealed record ProvisionServiceCommand(int ServiceId);
