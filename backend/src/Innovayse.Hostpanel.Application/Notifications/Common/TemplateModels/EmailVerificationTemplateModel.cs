namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the email address verification email.</summary>
/// <param name="VerificationLink">The absolute verification link, read as <c>{{ verification_link }}</c>.</param>
public sealed record EmailVerificationTemplateModel(string VerificationLink) : EmailTemplateModel;
