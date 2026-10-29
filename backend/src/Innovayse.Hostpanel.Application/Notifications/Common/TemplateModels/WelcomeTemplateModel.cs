namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>welcome</c> email sent on registration.</summary>
/// <param name="Client">The new client, read as <c>{{ client.email }}</c>.</param>
public sealed record WelcomeTemplateModel(WelcomeClientTemplateData Client) : EmailTemplateModel;
