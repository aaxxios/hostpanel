namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the invitation to manage a client account.</summary>
/// <param name="InviteLink">The absolute link that accepts the invitation, read as <c>{{ invite_link }}</c>.</param>
/// <param name="ClientName">The invited-to client's full name, read as <c>{{ client_name }}</c>.</param>
/// <param name="FirstName">The invitee's first name, read as <c>{{ first_name }}</c>.</param>
public sealed record UserInviteTemplateModel(string InviteLink, string ClientName, string FirstName) : EmailTemplateModel;
