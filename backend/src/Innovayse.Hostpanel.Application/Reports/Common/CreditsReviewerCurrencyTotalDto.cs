namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The Credits Reviewer totals of one currency.</summary>
/// <param name="Currency">The ISO 4217 code the credits are in.</param>
/// <param name="TotalCount">The number of matching credits in this currency.</param>
/// <param name="TotalAmount">Their sum.</param>
public sealed record CreditsReviewerCurrencyTotalDto(
    string Currency,
    int TotalCount,
    decimal TotalAmount);
