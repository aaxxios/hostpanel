namespace Innovayse.Hostpanel.Application.Services.Commands.RetireTerminatedService;

/// <summary>
/// Settles what a terminated service leaves behind: closes its open cancellation requests and
/// revokes its SSH keys on the server each was issued on. Idempotent.
/// </summary>
/// <param name="ServiceId">The terminated service's primary key.</param>
public sealed record RetireTerminatedServiceCommand(int ServiceId);
