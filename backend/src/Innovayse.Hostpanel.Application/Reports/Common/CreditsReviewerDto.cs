using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full Credits Reviewer report result.</summary>
/// <param name="Totals">The count and sum of the matching credits per currency, over every match.</param>
/// <param name="Rows">One page of the matching credits, newest first.</param>
public sealed record CreditsReviewerDto(
    IReadOnlyList<CreditsReviewerCurrencyTotalDto> Totals,
    PaginatedList<CreditsReviewerRowDto> Rows);
