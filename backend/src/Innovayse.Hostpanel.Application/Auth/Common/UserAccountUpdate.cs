namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// An operator's edit of one person's account: name, preferred UI language and sign-in address,
/// applied together.
/// </summary>
/// <param name="Subject">Whose account to update.</param>
/// <param name="FirstName">The new given name.</param>
/// <param name="LastName">The new family name.</param>
/// <param name="Email">The sign-in address; moved only when it differs from the stored one.</param>
/// <param name="Language">Preferred UI language, or <see langword="null"/> to leave it unset.</param>
public sealed record UserAccountUpdate(string Subject, string FirstName, string LastName, string Email, string? Language);
