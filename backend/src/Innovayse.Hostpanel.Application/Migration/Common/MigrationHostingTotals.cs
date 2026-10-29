namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>How many catalogue and provisioned hosting records the legacy source holds.</summary>
/// <param name="Products">Products available to pull.</param>
/// <param name="Services">Provisioned client services available to pull.</param>
/// <param name="Domains">Registered domains available to pull.</param>
public sealed record MigrationHostingTotals(int Products = 0, int Services = 0, int Domains = 0);
