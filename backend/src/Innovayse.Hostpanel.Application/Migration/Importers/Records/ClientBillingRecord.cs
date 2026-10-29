namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>How one client is billed, as the source plugin describes it.</summary>
/// <param name="Currency">ISO 4217 code of the currency the client is billed in.</param>
/// <param name="TaxExempt">Whether the client is exempt from tax on invoices.</param>
/// <param name="CreditBalance">Credit the client holds in the source, in their currency; carried over as account credit.</param>
internal sealed record ClientBillingRecord(string? Currency, bool TaxExempt, decimal CreditBalance);
