using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Commands.AddMySshKey;

/// <summary>
/// Adds an ED25519 public key to one of the caller's own services. Carries no server, account or
/// path: those are resolved from the service.
/// </summary>
/// <param name="ServiceId">The service, which must belong to the caller.</param>
/// <param name="Label">Friendly label.</param>
/// <param name="PublicKey">The public key line as pasted.</param>
public sealed record AddMySshKeyCommand(int ServiceId, string Label, string PublicKey) : ICallerScopedServiceMessage;
