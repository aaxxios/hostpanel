namespace Innovayse.Hostpanel.Application.Services.Commands.SuspendServiceSshKeys;

/// <summary>
/// Takes a suspended service's installed SSH keys off the server each was issued on. Control panels
/// suspend an account by locking its password, which does not stop a key from logging in. Dispatched
/// when a service is suspended, never routed. Idempotent: only installed keys are touched.
/// </summary>
/// <param name="ServiceId">The suspended service's primary key.</param>
public sealed record SuspendServiceSshKeysCommand(int ServiceId);
