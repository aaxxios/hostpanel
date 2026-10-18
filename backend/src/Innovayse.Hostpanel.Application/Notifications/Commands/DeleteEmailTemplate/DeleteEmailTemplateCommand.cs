namespace Innovayse.Hostpanel.Application.Notifications.Commands.DeleteEmailTemplate;

/// <summary>Command to permanently remove an email template.</summary>
/// <param name="Id">The identifier of the template to delete.</param>
public sealed record DeleteEmailTemplateCommand(int Id);
