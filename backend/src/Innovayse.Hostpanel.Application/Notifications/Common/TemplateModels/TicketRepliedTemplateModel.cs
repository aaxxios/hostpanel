namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>ticket-replied</c> email.</summary>
/// <param name="Ticket">The ticket, read as <c>{{ ticket.* }}</c>.</param>
public sealed record TicketRepliedTemplateModel(TicketTemplateData Ticket) : EmailTemplateModel;
