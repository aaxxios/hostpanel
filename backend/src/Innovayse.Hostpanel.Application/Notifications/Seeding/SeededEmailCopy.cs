namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>
/// The words of one language version of a seeded branded email: everything
/// <see cref="SeededEmailLayout"/> puts into the shared card, Liquid variables allowed.
/// </summary>
/// <param name="Language">The ISO 639-1 code the copy is written in.</param>
/// <param name="Heading">The card heading.</param>
/// <param name="Paragraphs">The card text, one paragraph per entry; inline HTML allowed.</param>
/// <param name="ButtonUrl">The button link (usually a Liquid variable), or <see langword="null"/> for no button.</param>
/// <param name="ButtonText">The button label, or <see langword="null"/> for no button.</param>
/// <param name="Note">The small print under the card text, or <see langword="null"/> for none.</param>
/// <param name="EchoButtonUrl">
/// <see langword="true"/> to repeat the button link as text under the note, for mail clients that
/// do not render the button.
/// </param>
public sealed record SeededEmailCopy(
    string Language,
    string Heading,
    IReadOnlyList<string> Paragraphs,
    string? ButtonUrl,
    string? ButtonText,
    string? Note,
    bool EchoButtonUrl);
