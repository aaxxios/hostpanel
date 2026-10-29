namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>One language version of a seeded branded template: its subject and its card copy.</summary>
/// <param name="Subject">The subject line, Liquid variables allowed.</param>
/// <param name="Copy">The words of the card, carrying the version's language.</param>
public sealed record SeededEmailVersion(string Subject, SeededEmailCopy Copy);
