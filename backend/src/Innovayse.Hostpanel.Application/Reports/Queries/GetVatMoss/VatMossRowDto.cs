namespace Innovayse.Hostpanel.Application.Reports.Queries.GetVatMoss;

/// <summary>One country row in the VAT MOSS report.</summary>
/// <param name="CountryName">The country name.</param>
/// <param name="CountryCode">The country code.</param>
/// <param name="VatRate">The vat rate.</param>
/// <param name="NumberOfInvoices">The number of invoices.</param>
/// <param name="TotalExclVat">The total excl vat.</param>
/// <param name="TotalVatCollected">The total vat collected.</param>
/// <param name="Currency">The currency.</param>
public record VatMossRowDto(
    string CountryName,
    string CountryCode,
    decimal VatRate,
    int NumberOfInvoices,
    decimal TotalExclVat,
    decimal TotalVatCollected,
    string Currency);
