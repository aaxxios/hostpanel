namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// A change to the name and preferred UI language of one person's account.
/// </summary>
/// <param name="Subject">Whose profile to update.</param>
/// <param name="FirstName">The new given name.</param>
/// <param name="LastName">The new family name.</param>
/// <param name="Language">Preferred UI language, or <see langword="null"/> to leave it unset.</param>
public sealed record UserProfileUpdate(string Subject, string FirstName, string LastName, string? Language);
