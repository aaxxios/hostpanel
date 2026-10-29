namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>The newly registered client as the welcome email reads it.</summary>
/// <param name="Email">The client's sign-in address, read as <c>email</c>.</param>
public sealed record WelcomeClientTemplateData(string Email);
