using Innovayse.Hostpanel.Domain.Orders.Enums;
namespace Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;

/// <summary>Query to retrieve a paged list of orders with optional status filtering.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Status">Optional status filter string (parsed to <see cref="Domain.Orders.Enums.OrderStatus"/>).</param>
public sealed record ListOrdersQuery(int Page, int PageSize, string? Status);
