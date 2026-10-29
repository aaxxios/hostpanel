namespace Innovayse.Hostpanel.Application.Services.Commands.RestoreServiceSshKeys;

/// <summary>
/// Puts an unsuspended service's suspended SSH keys back on the server each was issued on, under the
/// same id. Dispatched when the service is unsuspended and again once its hosting account is
/// unlocked on the panel, because the two are not ordered and an agent refuses keys for a locked
/// account. Never routed. Idempotent: only suspended keys are touched.
/// </summary>
/// <param name="ServiceId">The unsuspended service's primary key.</param>
public sealed record RestoreServiceSshKeysCommand(int ServiceId);
