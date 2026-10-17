namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePackage;

/// <summary>Changes the hosting package assigned to a provisioned account on its server.</summary>
/// <param name="ServiceId">Client service primary key. Overlaid from the route.</param>
/// <param name="NewPackage">The name of the new hosting package to assign.</param>
public sealed record ChangePackageCommand(int ServiceId, string NewPackage);
