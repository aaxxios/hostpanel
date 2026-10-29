using Innovayse.Hostpanel.Domain.Orders.Enums;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;

/// <summary>Query to retrieve a paged list of orders with optional status filtering.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Status">Optional status filter; an unknown value is refused when the query is bound, never ignored.</param>
public sealed record ListOrdersQuery(int Page, int PageSize, OrderStatus? Status);
