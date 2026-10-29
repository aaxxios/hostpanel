namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A service seeded for a client with no server assigned.
/// </summary>
/// <param name="ClientId">The owning client's id.</param>
/// <param name="ServiceId">The service's id.</param>
public sealed record SeededUnassignedService(int ClientId, int ServiceId);
