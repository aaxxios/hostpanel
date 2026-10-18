namespace Innovayse.Hostpanel.Application.Support.Queries.GetTicket;

/// <summary>Query to retrieve a single support ticket with all its replies.</summary>
/// <param name="Id">The ticket primary key.</param>
public sealed record GetTicketQuery(int Id);
