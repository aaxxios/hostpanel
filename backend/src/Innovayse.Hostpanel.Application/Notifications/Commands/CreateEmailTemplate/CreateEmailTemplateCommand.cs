namespace Innovayse.Hostpanel.Application.Notifications.Commands.CreateEmailTemplate;

/// <summary>Command to create a new email template, or a new language version of an existing one.</summary>
/// <param name="Slug">Slug used to look up the template; unique together with <paramref name="Language"/>.</param>
/// <param name="Language">ISO 639-1 code of the language the subject and body are written in.</param>
/// <param name="Subject">Email subject line, may include Liquid variables.</param>
/// <param name="Body">Email body, may include Liquid variables.</param>
/// <param name="Description">Optional human-readable description of the template's purpose.</param>
public sealed record CreateEmailTemplateCommand(string Slug, string Language, string Subject, string Body, string? Description);
