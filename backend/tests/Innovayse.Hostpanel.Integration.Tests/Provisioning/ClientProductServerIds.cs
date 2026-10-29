namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// The ids of a client, a product and a server seeded together.
/// </summary>
/// <param name="ClientId">The client's id.</param>
/// <param name="ProductId">The product's id.</param>
/// <param name="ServerId">The server's id.</param>
public sealed record ClientProductServerIds(int ClientId, int ProductId, int ServerId);
