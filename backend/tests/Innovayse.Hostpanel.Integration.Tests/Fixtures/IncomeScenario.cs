namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// What <see cref="IncomeTestSeeds.SeedAsync"/> put in the database, so an income report test can
/// find its own rows and know the currency the figures come back in.
/// </summary>
/// <param name="Year">The calendar year every seeded movement falls in.</param>
/// <param name="BaseCurrency">ISO 4217 code of the installation's base currency.</param>
/// <param name="BaseClientId">The client billed in the base currency.</param>
/// <param name="ForeignClientId">The client billed in <see cref="IncomeTestSeeds.ForeignCurrency"/>.</param>
/// <param name="ProductId">The product every seeded invoice line and order line is for.</param>
public sealed record IncomeScenario(int Year, string BaseCurrency, int BaseClientId, int ForeignClientId, int ProductId);
