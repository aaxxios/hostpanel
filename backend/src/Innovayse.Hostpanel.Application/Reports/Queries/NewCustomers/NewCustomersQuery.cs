namespace Innovayse.Hostpanel.Application.Reports.Queries.NewCustomers;

/// <summary>Query for the New Customers report.</summary>
/// <param name="Year">
/// Year to report on, or <see langword="null"/> for the current UTC year. Nullable so the
/// controller can bind this query directly from the route's optional <c>year</c> parameter
/// instead of resolving the default itself, which would put logic in the HTTP entry point.
/// </param>
public sealed record NewCustomersQuery(int? Year = null);
