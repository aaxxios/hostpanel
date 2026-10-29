namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An active service seeded on a server for a client.
/// </summary>
/// <param name="ClientId">The owning client's id.</param>
/// <param name="ServerId">The id of the server the service runs on.</param>
/// <param name="ServiceId">The service's id.</param>
public sealed record SeededActiveService(int ClientId, int ServerId, int ServiceId);
