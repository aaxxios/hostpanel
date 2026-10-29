namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>Full TLD configuration detail for admin views.</summary>
/// <param name="Id">The unique TLD configuration identifier.</param>
/// <param name="Tld">TLD extension without the leading dot (e.g. "am", "com").</param>
/// <param name="RegistrarModule">The registrar module name responsible for this TLD.</param>
/// <param name="Currency">ISO 4217 currency code for cost prices (registrar currency).</param>
/// <param name="SellCurrency">ISO 4217 currency code for sell prices (client-facing currency).</param>
/// <param name="IsEnabled">Whether this TLD is enabled for sale to clients.</param>
/// <param name="SortOrder">Display sort order (ascending).</param>
/// <param name="Categories">Marketing categories this TLD belongs to.</param>
/// <param name="Costs">Registrar cost for each action, keyed by period in years.</param>
/// <param name="Sells">Client-facing sell price for each action, keyed by period in years.</param>
/// <param name="LastSyncedAt">UTC timestamp of the last successful price sync, or null if never synced.</param>
/// <param name="CreatedAt">UTC timestamp when this configuration was created.</param>
public record TldConfigDto(
    int Id,
    string Tld,
    string RegistrarModule,
    string Currency,
    string SellCurrency,
    bool IsEnabled,
    int SortOrder,
    List<string> Categories,
    TldPriceTableDto Costs,
    TldPriceTableDto Sells,
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset CreatedAt);
