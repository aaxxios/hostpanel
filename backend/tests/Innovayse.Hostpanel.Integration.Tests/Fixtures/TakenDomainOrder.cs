namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A Pending order whose first line registers a domain the registrar refuses and whose second line
/// buys hosting.
/// </summary>
/// <param name="OrderId">The order's id.</param>
/// <param name="ClientId">The ordering client.</param>
/// <param name="ProductId">The hosting product of the second line.</param>
/// <param name="Domain">The domain the registrar refuses.</param>
public sealed record TakenDomainOrder(int OrderId, int ClientId, int ProductId, string Domain);
