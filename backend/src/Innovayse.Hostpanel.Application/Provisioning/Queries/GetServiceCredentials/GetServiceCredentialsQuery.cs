namespace Innovayse.Hostpanel.Application.Provisioning.Queries.GetServiceCredentials;

/// <summary>Retrieves the current login credentials for a provisioned hosting service.</summary>
/// <param name="ServiceId">The client service identifier. Overlaid from the route.</param>
public sealed record GetServiceCredentialsQuery(int ServiceId);
