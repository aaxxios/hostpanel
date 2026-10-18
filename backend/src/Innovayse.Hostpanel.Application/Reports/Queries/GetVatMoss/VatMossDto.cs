namespace Innovayse.Hostpanel.Application.Reports.Queries.GetVatMoss;

/// <summary>VAT MOSS Settlement Data report result.</summary>
/// <param name="PeriodLabel">The period label.</param>
/// <param name="Rows">The rows entries.</param>
public record VatMossDto(
    string PeriodLabel,
    IReadOnlyList<VatMossRowDto> Rows);
