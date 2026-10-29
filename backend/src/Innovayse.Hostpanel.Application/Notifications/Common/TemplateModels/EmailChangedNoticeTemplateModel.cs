namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the notice sent to the old address after an email change.</summary>
/// <param name="NewEmail">The address the account now signs in with, read as <c>{{ new_email }}</c>.</param>
public sealed record EmailChangedNoticeTemplateModel(string NewEmail) : EmailTemplateModel;
