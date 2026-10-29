namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full Direct Debit Processing report result.</summary>
/// <param name="Rows">The rows entries.</param>
public record DirectDebitDto(IReadOnlyList<DirectDebitRowDto> Rows);
