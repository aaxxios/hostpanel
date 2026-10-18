namespace Innovayse.Hostpanel.Application.Billing.Queries.GetTransactionById;

/// <summary>Query to fetch a single transaction by its primary key.</summary>
/// <param name="Id">Transaction primary key.</param>
public sealed record GetTransactionByIdQuery(int Id);
