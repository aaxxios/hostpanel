namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePassword;

/// <summary>Changes the hosting account password of a provisioned service on its server.</summary>
/// <param name="ServiceId">Client service primary key. Overlaid from the route.</param>
/// <param name="NewPassword">The new password to set.</param>
public sealed record ChangePasswordCommand(int ServiceId, string NewPassword);
