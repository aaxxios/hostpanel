namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// Asks for a service's suspended SSH keys to be put back on its server. Sent once the hosting
/// account itself has been unsuspended, because an agent refuses keys for a locked account and the
/// service's own unsuspension may be handled before the panel has unlocked it.
/// </summary>
/// <param name="ServiceId">The unsuspended service ID.</param>
/// <param name="ClientId">The owning client ID.</param>
public sealed record RestoreSuspendedSshKeys(int ServiceId, int ClientId);
