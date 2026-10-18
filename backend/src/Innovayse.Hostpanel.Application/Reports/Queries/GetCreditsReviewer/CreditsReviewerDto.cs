namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCreditsReviewer;

/// <summary>Full Credits Reviewer report result.</summary>
/// <param name="TotalCount">The total count.</param>
/// <param name="TotalAmount">The total amount.</param>
/// <param name="Rows">The rows entries.</param>
public record CreditsReviewerDto(
    int TotalCount,
    decimal TotalAmount,
    IReadOnlyList<CreditsReviewerRowDto> Rows);
