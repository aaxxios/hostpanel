namespace Innovayse.Hostpanel.Application.Billing.Queries.ListTransactions;

/// <summary>Query to list transactions. Pass null ClientId to list all clients.</summary>
/// <param name="ClientId">The client's primary key, or null for all clients.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record ListTransactionsQuery(int? ClientId, int Page = 1, int PageSize = 25);
