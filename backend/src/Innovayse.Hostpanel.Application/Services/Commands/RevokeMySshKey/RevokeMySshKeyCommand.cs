using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Commands.RevokeMySshKey;

/// <summary>Revokes one SSH key of one of the caller's own services.</summary>
/// <param name="ServiceId">The service, which must belong to the caller.</param>
/// <param name="KeyId">The key's public id; must belong to that service.</param>
public sealed record RevokeMySshKeyCommand(int ServiceId, Guid KeyId) : ICallerScopedServiceMessage;
