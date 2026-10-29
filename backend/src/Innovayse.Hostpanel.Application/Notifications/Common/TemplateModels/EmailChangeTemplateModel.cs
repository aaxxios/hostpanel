namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the email sent to a new address to confirm an email change.</summary>
/// <param name="ConfirmationLink">The absolute confirmation link, read as <c>{{ confirmation_link }}</c>.</param>
public sealed record EmailChangeTemplateModel(string ConfirmationLink) : EmailTemplateModel;
