namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>user-password-reset</c> email.</summary>
/// <param name="ResetLink">The absolute password reset link, read as <c>{{ reset_link }}</c>.</param>
public sealed record PasswordResetTemplateModel(string ResetLink) : EmailTemplateModel;
