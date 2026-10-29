namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A signed-in owner of a service on a server, for the SSH key use cases.
/// </summary>
/// <param name="Http">The HTTP client carrying the owner's bearer token.</param>
/// <param name="ClientId">The owner's client id.</param>
/// <param name="ServerId">The id of the server the service runs on.</param>
/// <param name="ServiceId">The service's id.</param>
public sealed record SshOwnerScenario(HttpClient Http, int ClientId, int ServerId, int ServiceId);
