namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A service and a bearer token of the client user who owns it.
/// </summary>
/// <param name="ServiceId">The service's id.</param>
/// <param name="OwnerToken">The owner's bearer token.</param>
public sealed record OwnedService(int ServiceId, string OwnerToken);
