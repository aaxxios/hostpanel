namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A client row seeded straight into the database.
/// </summary>
/// <param name="ClientId">The new client's primary key.</param>
/// <param name="UserId">The identity subject the client row is linked to.</param>
public sealed record SeededClient(int ClientId, string UserId);
