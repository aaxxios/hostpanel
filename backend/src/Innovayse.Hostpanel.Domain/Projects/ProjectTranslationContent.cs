namespace Innovayse.Hostpanel.Domain.Projects;

/// <summary>The text of one locale of a project, handed to <see cref="Project.ReplaceTranslations"/>.</summary>
/// <param name="Title">The project's headline in this locale.</param>
/// <param name="Summary">The one-line teaser shown on cards; null when the project has none.</param>
/// <param name="Description">The long body shown on the project page; null when the project has none.</param>
public sealed record ProjectTranslationContent(string Title, string? Summary, string? Description);
