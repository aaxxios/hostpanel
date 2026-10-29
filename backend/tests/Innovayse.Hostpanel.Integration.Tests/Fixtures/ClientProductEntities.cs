using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A client and a product seeded together, as entities.
/// </summary>
/// <param name="Client">The seeded client.</param>
/// <param name="Product">The seeded product.</param>
public sealed record ClientProductEntities(Client Client, Product Product);
