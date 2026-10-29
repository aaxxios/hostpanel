using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full Sales Tax Liability report result.</summary>
/// <param name="Totals">The invoice count, amount invoiced and tax liability per invoice currency, over every matching invoice.</param>
/// <param name="Rows">One page of the matching invoices, in the order they were paid.</param>
public sealed record SalesTaxReportDto(
    IReadOnlyList<SalesTaxCurrencyTotalDto> Totals,
    PaginatedList<SalesTaxRowDto> Rows);
