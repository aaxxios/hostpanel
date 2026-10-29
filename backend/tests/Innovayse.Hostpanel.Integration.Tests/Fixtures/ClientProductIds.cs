namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// The ids of a client and a product seeded together.
/// </summary>
/// <param name="ClientId">The seeded client's id.</param>
/// <param name="ProductId">The seeded product's id.</param>
public sealed record ClientProductIds(int ClientId, int ProductId);
