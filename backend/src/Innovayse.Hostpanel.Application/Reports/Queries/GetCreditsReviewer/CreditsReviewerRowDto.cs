namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCreditsReviewer;

/// <summary>One row in the Credits Reviewer report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="Date">The date.</param>
/// <param name="Description">The description.</param>
/// <param name="Amount">The amount.</param>
/// <param name="AdminUser">The admin user.</param>
public record CreditsReviewerRowDto(
    int Id,
    int ClientId,
    string ClientName,
    string Date,
    string Description,
    decimal Amount,
    string? AdminUser);
