namespace Innovayse.Hostpanel.Application.Reports.Queries.GetTicketRatingsReviewer;

/// <summary>Ticket Ratings Reviewer report result.</summary>
/// <param name="Rows">One entry per rated reply matching the filters.</param>
public record TicketRatingsReviewerDto(IReadOnlyList<TicketRatingRowDto> Rows);
