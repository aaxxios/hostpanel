namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCreditsReviewer;

/// <summary>Query for the Credits Reviewer report.</summary>
/// <param name="ClientId">Client to narrow to, or null for every client.</param>
/// <param name="From">Earliest credit day to include (UTC).</param>
/// <param name="To">Latest credit day to include (UTC), inclusive.</param>
/// <param name="MinAmount">Smallest credit amount to include.</param>
/// <param name="MaxAmount">Largest credit amount to include.</param>
/// <param name="Page">One-based page number of the credit rows.</param>
/// <param name="PageSize">Credit rows per page.</param>
public sealed record GetCreditsReviewerQuery(
    int? ClientId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    int Page = 1,
    int PageSize = 50);
