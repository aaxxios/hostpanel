namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>ticket-created</c> email sent to the department.</summary>
/// <param name="Ticket">The ticket, read as <c>{{ ticket.* }}</c>.</param>
public sealed record TicketCreatedTemplateModel(TicketTemplateData Ticket) : EmailTemplateModel;
