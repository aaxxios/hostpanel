namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>An active service seeded for a renewal test, with its client and product.</summary>
/// <param name="ClientId">The owning client's id.</param>
/// <param name="ProductId">The product's id.</param>
/// <param name="ServiceId">The service's id.</param>
public sealed record SeededRenewingService(int ClientId, int ProductId, int ServiceId);
