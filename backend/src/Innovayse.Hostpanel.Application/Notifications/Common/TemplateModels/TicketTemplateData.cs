namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A support ticket as an email template reads it.</summary>
/// <param name="Id">The ticket id, read as <c>id</c>.</param>
/// <param name="Subject">The ticket subject as the client typed it, read as <c>subject</c>.</param>
public sealed record TicketTemplateData(int Id, string Subject);
